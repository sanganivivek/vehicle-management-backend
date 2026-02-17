using vehicle_management_backend.Core.Models;
namespace vehicle_management_backend.Infrastructure.Repositories.Interfaces
{
    public interface IModelRespository
    {
        Task<List<Model>> GetAllAsync();
        Task<Model?> GetByIdAsync(Guid id);
        Task<List<Model>> GetByBrandIdAsync(Guid brandId); 
        Task<(List<Model> Items, int TotalCount)> GetPagedAsync(string? search, Guid? brandId, int page, int pageSize, string? sortBy, string? sortOrder);
        Task AddAsync(Model model);
        Task UpdateAsync(Model model);
        Task DeleteAsync(Guid id);
        Task BulkAddAsync(IEnumerable<Model> models);
    }
}