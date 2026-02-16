using vehicle_management_backend.Core.Models;
namespace vehicle_management_backend.Application.Services.Interfaces
{
    public interface IVehicleService
    {
        Task<IList<VehicleMaster>> GetAllAsync();
        Task<(IList<VehicleMaster> Items, int TotalCount)> GetVehiclesAsync(string? search, string? brand, int? status, bool? isActive, string? sortBy, string? sortOrder, int page, int pageSize);
        Task<VehicleMaster?> GetByIdAsync(Guid id);
        Task<VehicleMaster?> GetByRegNoAsync(string regNo);
        Task CreateAsync(VehicleMaster vehicle);
        Task UpdateAsync(VehicleMaster vehicle);
        Task DeleteAsync(Guid id);
        
        // Dashboard Statistics
        Task<(int TotalCount, int ActiveCount, IList<VehicleMaster> RecentVehicles)> GetDashboardStatsAsync();
        
        // Stored Procedure Methods
        Task<IList<VehicleMaster>> GetAllSPAsync();
        Task CreateSPAsync(VehicleMaster vehicle);
    }
}