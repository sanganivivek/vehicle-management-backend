using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using vehicle_management_backend.Infrastructure.Data;
using vehicle_management_backend.Core.Enums;
using vehicle_management_backend.Core.Models;

namespace vehicle_management_backend.Infrastructure.Services
{
    public class VehicleStatusUpdater : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<VehicleStatusUpdater> _logger;

        public VehicleStatusUpdater(IServiceProvider serviceProvider, ILogger<VehicleStatusUpdater> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await UpdateVehicleStatusesAsync();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error updating vehicle statuses");
                }

                // Run every 5 minutes (adjust as needed)
                await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
            }
        }

        private async Task UpdateVehicleStatusesAsync()
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                var currentTime = DateTime.UtcNow;

                // 1. Find Vehicles that SHOULD be Rented (Active Confirmed Booking)
                // Logic: Valid Booking exists NOW, but Vehicle is not marked Rented
                var vehiclesToRent = await dbContext.Bookings
                    .Where(b => b.Status == 1 // Confirmed
                             && b.StartDate <= currentTime 
                             && b.EndDate >= currentTime
                             && b.Vehicle != null
                             && b.Vehicle.CurrentStatus != (int)VehicleStatus.Rented
                             && b.Vehicle.CurrentStatus != (int)VehicleStatus.Inmaintance) // Don't override Maintenance
                    .Select(b => b.Vehicle)
                    .Distinct()
                    .ToListAsync();

                if (vehiclesToRent.Any())
                {
                    foreach (var vehicle in vehiclesToRent)
                    {
                        if(vehicle != null) vehicle.CurrentStatus = (int)VehicleStatus.Rented;
                    }
                    _logger.LogInformation($"Marking {vehiclesToRent.Count} vehicles as Rented.");
                }

                // 2. Find Vehicles that SHOULD be Available (Booking Expired/Completed)
                // Logic: Vehicle is 'Rented', but NO active booking exists for it right now
                // Note: We perform this check on vehicles currently marked 'Rented' (1)
                var rentedVehicles = await dbContext.Vehicles
                    .Where(v => v.CurrentStatus == (int)VehicleStatus.Rented)
                    .ToListAsync();

                foreach (var vehicle in rentedVehicles)
                {
                    // Check if this vehicle has any active booking right now
                    bool hasActiveBooking = await dbContext.Bookings
                        .AnyAsync(b => b.VehicleId == vehicle.VehicleId 
                                    && b.Status == 1 // Confirmed
                                    && b.StartDate <= currentTime 
                                    && b.EndDate >= currentTime);

                    if (!hasActiveBooking)
                    {
                        vehicle.CurrentStatus = (int)VehicleStatus.Available;
                        _logger.LogInformation($"Marking vehicle {vehicle.RegNo} as Available (Booking ended).");
                    }
                }

                await dbContext.SaveChangesAsync();
            }
        }
    }
}
