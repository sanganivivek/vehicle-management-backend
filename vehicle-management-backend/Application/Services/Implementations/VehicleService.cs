using vehicle_management_backend.Application.Services.Interfaces;
using vehicle_management_backend.Core.Models;
using vehicle_management_backend.Infrastructure.Repositories.Interfaces;
namespace vehicle_management_backend.Application.Services.Implementations
{
    public class VehicleService : IVehicleService
    {
        private readonly IVehicleRepository _vehicleRepository;
        private readonly IActivityLogService _activityLogService;
        
        public VehicleService(IVehicleRepository vehicleRepository, IActivityLogService activityLogService)
        {
            _vehicleRepository = vehicleRepository;
            _activityLogService = activityLogService;
        }
        public async Task<IList<VehicleMaster>> GetAllAsync()
        {
            return await _vehicleRepository.GetAllAsync();
        }

        public async Task<(IList<VehicleMaster> Items, int TotalCount)> GetVehiclesAsync(string? search, string? brand, int? status, string? sortBy, string? sortOrder, int page, int pageSize)
        {
            var result = await _vehicleRepository.GetVehiclesAsync(search, brand, status, sortBy, sortOrder, page, pageSize);
            return (result.Items, result.TotalCount);
        }
        
        public async Task<VehicleMaster?> GetByIdAsync(Guid id)
        {
            return await _vehicleRepository.GetByIdAsync(id);
        }

        public async Task<VehicleMaster?> GetByRegNoAsync(string regNo)
        {
            return await _vehicleRepository.GetByRegNoAsync(regNo);
        }

        public async Task CreateAsync(VehicleMaster vehicle)
        {
            await _vehicleRepository.AddAsync(vehicle);
            await _activityLogService.LogCreateAsync($"Created new Vehicle '{vehicle.RegNo}'");
        }
        public async Task UpdateAsync(VehicleMaster vehicle)
        {
            await _vehicleRepository.UpdateAsync(vehicle);
            await _activityLogService.LogUpdateAsync($"Updated Vehicle '{vehicle.RegNo}'");
        }
        public async Task DeleteAsync(Guid id)
        {
            var vehicle = await _vehicleRepository.GetByIdAsync(id);
            await _vehicleRepository.DeleteAsync(id);
            if (vehicle != null)
            {
                await _activityLogService.LogDeleteAsync($"Deleted Vehicle '{vehicle.RegNo}'");
            }
        }
        
        // Dashboard Statistics
        public async Task<(int TotalCount, int ActiveCount, IList<VehicleMaster> RecentVehicles)> GetDashboardStatsAsync()
        {
            var result = await _vehicleRepository.GetDashboardStatsAsync();
            return (result.TotalCount, result.ActiveCount, result.RecentVehicles);
        }
        
        // Stored Procedure Methods
        public async Task<IList<VehicleMaster>> GetAllSPAsync()
        {
            return await _vehicleRepository.GetAllViaStoredProcAsync();
        }
        
        public async Task CreateSPAsync(VehicleMaster vehicle)
        {
            await _vehicleRepository.CreateViaStoredProcAsync(vehicle);
        }
    }
}