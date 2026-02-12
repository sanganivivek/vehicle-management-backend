using vehicle_management_backend.Application.Services.Interfaces;
using vehicle_management_backend.Core.DTOs;
using vehicle_management_backend.Core.Models;
using vehicle_management_backend.Infrastructure.Repositories.Interfaces;
namespace vehicle_management_backend.Application.Services.Implementations
{
    public class ModelService : IModelService
    {
        private readonly IModelRespository _modelRepository;
        private readonly IActivityLogService _activityLogService;
        
        public ModelService(IModelRespository modelRepository, IActivityLogService activityLogService)
        {
            _modelRepository = modelRepository;
            _activityLogService = activityLogService;
        }
        public async Task<List<Model>> GetModelsByBrandAsync(Guid brandId)
        {
            return await _modelRepository.GetByBrandIdAsync(brandId);
        }
        public async Task<List<ModelDTO>> GetModelsAsync()
        {
            var models = await _modelRepository.GetAllAsync();
            return models.Select(m => new ModelDTO
            {
                ModelId = m.ModelId,
                ModelCode = m.ModelCode,
                ModelName = m.ModelName,

                Description = m.Description,
                BrandId = m.BrandId
            }).ToList();
        }
        public async Task CreateAsync(Model model)
        {
            await _modelRepository.AddAsync(model);
            await _activityLogService.LogCreateAsync($"Created new Model '{model.ModelName}'");
        }

        public async Task<ModelDTO?> GetByIdAsync(Guid id)
        {
            var model = await _modelRepository.GetByIdAsync(id);
            if (model == null) return null;

            return new ModelDTO
            {
                ModelId = model.ModelId,
                ModelCode = model.ModelCode,
                ModelName = model.ModelName,

                Description = model.Description,
                BrandId = model.BrandId
            };
        }

        public async Task<Model?> GetModelByIdAsync(Guid id)
        {
            return await _modelRepository.GetByIdAsync(id);
        }

        public async Task UpdateAsync(Guid id, CreateModelDTO dto)
        {
            var model = await _modelRepository.GetByIdAsync(id);
            if (model != null)
            {
                model.BrandId = dto.BrandId;
                model.ModelCode = dto.ModelCode;
                model.ModelName = dto.Name;

                model.Description = dto.Description;
                await _modelRepository.UpdateAsync(model);
                await _activityLogService.LogUpdateAsync($"Updated Model '{dto.Name}'");
            }
        }

        public async Task DeleteAsync(Guid id)
        {
            var model = await _modelRepository.GetByIdAsync(id);
            await _modelRepository.DeleteAsync(id);
            if (model != null)
            {
                await _activityLogService.LogDeleteAsync($"Deleted Model '{model.ModelName}'");
            }
        }
    }
}