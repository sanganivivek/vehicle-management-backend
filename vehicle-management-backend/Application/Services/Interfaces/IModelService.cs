using vehicle_management_backend.Core.DTOs;
using vehicle_management_backend.Core.Models;
namespace vehicle_management_backend.Application.Services.Interfaces
{
    public interface IModelService
    {
        Task<List<Model>> GetModelsByBrandAsync(Guid brandId);
        Task<(List<ModelDTO> Items, int TotalCount)> GetPagedModelsAsync(string? search, Guid? brandId, int page, int pageSize, string? sortBy, string? sortOrder);
        Task<List<ModelDTO>> GetModelsAsync();
        Task<ModelDTO?> GetByIdAsync(Guid id);
        Task<Model?> GetModelByIdAsync(Guid id); // Added for validation
        Task CreateAsync(Model model);
        Task UpdateAsync(Guid id, CreateModelDTO dto);
        Task DeleteAsync(Guid id);
    }
}