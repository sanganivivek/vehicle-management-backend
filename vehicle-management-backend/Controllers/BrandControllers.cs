using Microsoft.AspNetCore.Mvc;
using vehicle_management_backend.Application.Services.Interfaces;
using vehicle_management_backend.Core.DTOs;

namespace vehicle_management_backend.Controllers
{
    [ApiController]
    [Route("api/brands")]
    public class Brand : ControllerBase
    {
        private readonly IBrandService _brandService;

        public Brand(IBrandService brandService)
        {
            _brandService = brandService;
        }

        [HttpPost]
        public async Task<IActionResult> Create(BrandDTO dto)
        {
            await _brandService.AddBrandAsync(dto);
            return Ok(new { message = "Brand created successfully" });
        }

        [HttpGet]
        public async Task<IActionResult> Get([FromQuery] string? search, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            try
            {
                var brands = await _brandService.GetBrandsAsync();
                
                if (!string.IsNullOrEmpty(search))
                {
                    brands = brands.Where(b => b.BrandName.Contains(search, StringComparison.OrdinalIgnoreCase) || 
                                             b.BrandCode.Contains(search, StringComparison.OrdinalIgnoreCase)).ToList();
                }

                var totalCount = brands.Count();
                var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);
                
                var pagedBrands = brands
                    .OrderByDescending(b => b.BrandName) 
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToList();

                return Ok(new
                {
                    totalCount,
                    page,
                    data = pagedBrands,
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
        public async Task<IActionResult> Get(Guid id)
        {
            var brand = await _brandService.GetBrandByIdAsync(id);
            if (brand == null) return NotFound();
            return Ok(brand);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, BrandDTO dto)
        {
            await _brandService.UpdateBrandAsync(id, dto);
            return Ok(new { message = "Brand updated successfully" });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            try
            {
                // Check if any models reference this brand
                var modelService = HttpContext.RequestServices.GetService<IModelService>();
                if (modelService != null)
                {
                    var models = await modelService.GetModelsAsync();
                    var modelsUsingBrand = models.Where(m => m.BrandId == id).ToList();
                    
                    if (modelsUsingBrand.Any())
                    {
                        return BadRequest(new { 
                            error = "Cannot Delete Brand", 
                            message = $"Cannot delete this brand because {modelsUsingBrand.Count} model(s) are currently associated with it. Please delete or reassign those models first.",
                            modelCount = modelsUsingBrand.Count
                        });
                    }
                }

                await _brandService.DeleteBrandAsync(id);
                return Ok(new { message = "Brand deleted successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message, innerException = ex.InnerException?.Message });
            }
        }
    }
}