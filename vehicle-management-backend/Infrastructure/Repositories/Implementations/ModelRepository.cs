using Microsoft.EntityFrameworkCore;
using vehicle_management_backend.Core.Models;
using vehicle_management_backend.Infrastructure.Data;
using vehicle_management_backend.Infrastructure.Repositories.Interfaces;
namespace vehicle_management_backend.Infrastructure.Repositories.Implementations
{
    public class ModelRepository : IModelRespository
    {
        private readonly AppDbContext _context;
        public ModelRepository(AppDbContext context)
        {
            _context = context;
        }
        public async Task<List<Model>> GetAllAsync()
        {
            return await _context.Models.ToListAsync();
        }
        public async Task<List<Model>> GetByBrandIdAsync(Guid brandId)
        {
            return await _context.Models
                                 .Where(m => m.BrandId == brandId)
                                 .ToListAsync();
        }

        public async Task<(List<Model> Items, int TotalCount)> GetPagedAsync(string? search, Guid? brandId, int page, int pageSize, string? sortBy, string? sortOrder)
        {
            var query = _context.Models.Include(m => m.Brand).AsQueryable();

            // 1. Filter
            if (brandId.HasValue)
            {
                query = query.Where(m => m.BrandId == brandId.Value);
            }

            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(m => 
                    (m.ModelName != null && m.ModelName.Contains(search)) || 
                    (m.ModelCode != null && m.ModelCode.Contains(search))
                    // Note: Can't filter by BrandName here easily without Include/Join, 
                    // but usually filtering by Model properties is sufficient for this list.
                    // If BrandName filtering is critical, we need to Include(m => m.Brand) and filter.
                );
            }

            // 2. Sort
            if (!string.IsNullOrEmpty(sortBy))
            {
                switch (sortBy.ToLower())
                {
                    case "modelname":
                        query = sortOrder?.ToLower() == "desc" ? query.OrderByDescending(m => m.ModelName) : query.OrderBy(m => m.ModelName);
                        break;
                    case "modelcode":
                        query = sortOrder?.ToLower() == "desc" ? query.OrderByDescending(m => m.ModelCode) : query.OrderBy(m => m.ModelCode);
                        break;
                    default:
                        query = query.OrderBy(m => m.ModelName);
                        break;
                }
            }
            else
            {
                query = query.OrderBy(m => m.ModelName);
            }

            // 3. Paginate
            var totalCount = await query.CountAsync();
            var items = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

            return (items, totalCount);
        }
        public async Task AddAsync(Model model)
        {
            await _context.Models.AddAsync(model);
            await _context.SaveChangesAsync();
        }

        public async Task<Model?> GetByIdAsync(Guid id)
        {
            return await _context.Models.FindAsync(id);
        }

        public async Task UpdateAsync(Model model)
        {
            _context.Models.Update(model);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Guid id)
        {
            var model = await _context.Models.FindAsync(id);
            if (model != null)
            {
                _context.Models.Remove(model);
                await _context.SaveChangesAsync();
            }
        }
    }
}