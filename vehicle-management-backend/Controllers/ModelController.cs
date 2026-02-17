using Microsoft.AspNetCore.Mvc;
using vehicle_management_backend.Application.Services.Interfaces;
using vehicle_management_backend.Core.DTOs;
using vehicle_management_backend.Core.Models;
namespace vehicle_management_backend.Controllers
{
    [ApiController]
    [Route("api/models")]
    public class ModelController : ControllerBase
    {
        private readonly IModelService _modelService;
        private readonly IBrandService _brandService;
        
        public ModelController(IModelService modelService, IBrandService brandService)
        {
            _modelService = modelService;
            _brandService = brandService;
        }
        
        [HttpGet("by-brand/{brandId}")]
        public async Task<IActionResult> GetByBrand(Guid brandId)
        {
            var models = await _modelService.GetModelsByBrandAsync(brandId);
            var brands = await _brandService.GetBrandsAsync();
            
            var dtos = models.Select(m =>
            {
                var brand = brands.FirstOrDefault(b => b.BrandId == m.BrandId);
                return new ModelDTO
                {
                    ModelId = m.ModelId,
                    ModelCode = m.ModelCode,
                    ModelName = m.ModelName,

                    Description = m.Description,
                    BrandId = m.BrandId,
                    BrandName = brand?.BrandName,
                    BrandCode = brand?.BrandCode
                };
            });
            return Ok(dtos);
        }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] string? search, [FromQuery] Guid? brandId = null, [FromQuery] int page = 1, [FromQuery] int pageSize = 10, [FromQuery] string? sortBy = null, [FromQuery] string? sortOrder = "asc")
        {
            try
            {
                // Call the optimized service method
                var (pagedModels, totalCount) = await _modelService.GetPagedModelsAsync(search, brandId, page, pageSize, sortBy, sortOrder);
                
                // Fetch all brands only if needed for display (to populate BrandName)
                // Note: ideally we would Include(b => b.Brand) in the repo, but to minimize changes we fetch brands here.
                // However, optimization: if we filtered by brandId, we only need that one brand name.
                // If not, we still might need all brands to map names.
                // A better approach for list view is to have BrandName validation in frontend or minimal fetch.
                // But let's keep existing behavior of showing BrandName.
                
                var brands = await _brandService.GetBrandsAsync();
                
                // Map BrandName to the DTOs
                foreach (var model in pagedModels)
                {
                     var brand = brands.FirstOrDefault(b => b.BrandId == model.BrandId);
                     model.BrandName = brand?.BrandName ?? "Unknown";
                     model.BrandCode = brand?.BrandCode;
                }

                var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);

                return Ok(new
                {
                    totalCount,
                    page,
                    data = pagedModels,
                    totalPages,
                    totalRecords = totalCount,
                    pageSize
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var model = await _modelService.GetByIdAsync(id);
            if (model == null) return NotFound();
            
            var brands = await _brandService.GetBrandsAsync();
            var brand = brands.FirstOrDefault(b => b.BrandId == model.BrandId);
            
            var dto = new ModelDTO
            {
                ModelId = model.ModelId,
                ModelCode = model.ModelCode,
                ModelName = model.ModelName,

                Description = model.Description,
                BrandId = model.BrandId,
                BrandName = brand?.BrandName,
                BrandCode = brand?.BrandCode
            };
            
            return Ok(dto);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, CreateModelDTO dto)
        {
            await _modelService.UpdateAsync(id, dto);
            return Ok(new { message = "Model updated successfully" });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            try
            {
                // Check if any vehicles reference this model
                var vehicleService = HttpContext.RequestServices.GetService<IVehicleService>();
                if (vehicleService != null)
                {
                    var vehicles = await vehicleService.GetAllAsync();
                    var vehiclesUsingModel = vehicles.Where(v => v.ModelId == id).ToList();
                    
                    if (vehiclesUsingModel.Any())
                    {
                        return BadRequest(new { 
                            error = "Cannot Delete Model", 
                            message = $"Cannot delete this model because {vehiclesUsingModel.Count} vehicle(s) are currently using it. Please reassign or delete those vehicles first.",
                            vehicleCount = vehiclesUsingModel.Count
                        });
                    }
                }

                await _modelService.DeleteAsync(id);
                return Ok(new { message = "Model deleted successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message, innerException = ex.InnerException?.Message });
            }
        }
        
        [HttpPost]
        public async Task<IActionResult> Create(CreateModelDTO dto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }
                var model = new Model
                {
                    ModelId = Guid.NewGuid(),
                    BrandId = dto.BrandId,
                    ModelCode = dto.ModelCode,
                    ModelName = dto.Name,

                    Description = dto.Description
                };
                await _modelService.CreateAsync(model);

                return Ok(new { message = "Model created successfully", modelId = model.ModelId });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message, innerException = ex.InnerException?.Message });
            }
        }
    }
}