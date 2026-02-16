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

        // This function runs automatically in the background
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                await UpdateStatusesAsync();
                // Wait 1 minute before checking again
                await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
            }
        }

        private async Task UpdateStatusesAsync()
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                var currentTime = DateTime.UtcNow; // Uses Universal Time (UTC)

                // ---------------------------------------------------------
                // STEP 1: CHECK GUESTS OUT (Complete the Bookings)
                // ---------------------------------------------------------
                var finishedBookings = await dbContext.Bookings
                    .Where(b => b.Status == 1 && b.EndDate < currentTime) // Status 1 = Confirmed
                    .ToListAsync();

                if (finishedBookings.Any())
                {
                    foreach (var booking in finishedBookings)
                    {
                        booking.Status = 2; // Change Status to 2 (Completed)
                    }
                    await dbContext.SaveChangesAsync(); // Save this change to DB
                }

                // ---------------------------------------------------------
                // STEP 2: UPDATE ROOM STATUS (Update Vehicle Availability)
                // ---------------------------------------------------------

                // A. If a booking is active NOW, make vehicle RENTED
                var vehiclesToRent = await dbContext.Bookings
                    .Where(b => b.Status == 1
                             && b.StartDate <= currentTime
                             && b.EndDate >= currentTime)
                    .Select(b => b.Vehicle)
                    .Distinct()
                    .ToListAsync();

                foreach (var vehicle in vehiclesToRent)
                {
                    if (vehicle != null && vehicle.CurrentStatus == (int)VehicleStatus.Available)
                    {
                        vehicle.CurrentStatus = (int)VehicleStatus.Rented;
                    }
                }

                // B. If a vehicle is Rented but has NO active booking, make it AVAILABLE
                var rentedVehicles = await dbContext.Vehicles
                    .Where(v => v.CurrentStatus == (int)VehicleStatus.Rented)
                    .ToListAsync();

                foreach (var vehicle in rentedVehicles)
                {
                    // Is there anyone currently using this car?
                    bool isBusy = await dbContext.Bookings
                        .AnyAsync(b => b.VehicleId == vehicle.VehicleId
                                    && b.Status == 1 // Confirmed
                                    && b.StartDate <= currentTime
                                    && b.EndDate >= currentTime);

                    // If nobody is using it, mark it Available
                    if (!isBusy)
                    {
                        vehicle.CurrentStatus = (int)VehicleStatus.Available;
                    }
                }

                await dbContext.SaveChangesAsync();
            }
        }
    }
}