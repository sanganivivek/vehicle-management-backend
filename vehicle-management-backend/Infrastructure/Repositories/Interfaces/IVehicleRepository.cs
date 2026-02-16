using vehicle_management_backend.Core.Models;
namespace vehicle_management_backend.Infrastructure.Repositories.Interfaces
{
    public interface IVehicleRepository
    {
        Task<List<VehicleMaster>> GetAllAsync();
        Task<VehicleMaster?> GetByIdAsync(Guid id);
        Task<VehicleMaster?> GetByRegNoAsync(string regNo);
        Task<(List<VehicleMaster> Items, int TotalCount)> GetVehiclesAsync(string? search, string? brand, int? status, bool? isActive, string? sortBy, string? sortOrder, int page, int pageSize);
        Task AddAsync(VehicleMaster vehicle);
        Task UpdateAsync(VehicleMaster vehicle);
        Task DeleteAsync(Guid id);
        
        // Dashboard Statistics
        Task<(int TotalCount, int ActiveCount, List<VehicleMaster> RecentVehicles)> GetDashboardStatsAsync();
        
        // Stored Procedure Methods
        Task<List<VehicleMaster>> GetAllViaStoredProcAsync();
        Task CreateViaStoredProcAsync(VehicleMaster vehicle);
    }
}