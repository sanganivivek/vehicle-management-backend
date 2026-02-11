using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using vehicle_management_backend.Core.Models;
using vehicle_management_backend.Infrastructure.Data;
using vehicle_management_backend.Infrastructure.Repositories.Interfaces;

namespace vehicle_management_backend.Infrastructure.Repositories.Implementations
{
    public class VehicleRepository : IVehicleRepository
    {
        private readonly AppDbContext _context;
                                
        public VehicleRepository(AppDbContext context)
        {
            _context = context;
        }

        // 1. GET ALL VEHICLES
        public async Task<List<VehicleMaster>> GetAllAsync()
        {
            return await _context.Vehicles
                .Include(v => v.Brand)
                .Include(v => v.Model)
                .Include(v => v.Dealer)
                .OrderByDescending(v => v.CreatedAt)
                .ToListAsync();
        }

        // 1.1 GET VEHICLES WITH PAGINATION/FILTERING
        public async Task<(List<VehicleMaster> Items, int TotalCount)> GetVehiclesAsync(string? search, string? brand, int? status, string? sortBy, string? sortOrder, int page, int pageSize)
        {
            var query = _context.Vehicles
                .Include(v => v.Brand)
                .Include(v => v.Model)
                .Include(v => v.Dealer)
                .AsQueryable();

            // Filter by Status
            if (status.HasValue)
            {
                query = query.Where(v => v.CurrentStatus == status.Value);
            }

            // Filter by Brand (Name)
            if (!string.IsNullOrEmpty(brand))
            {
                query = query.Where(v => v.Brand.BrandName == brand); 
            }

            // Filter by Search (RegNo or Chassis)
            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(v => v.RegNo.Contains(search) || v.ChassisNumber.Contains(search));
            }

            // Get Total Count (Data needed for pagination UI)
            var totalCount = await query.CountAsync();

            // Sorting
            if (!string.IsNullOrEmpty(sortBy))
            {
                switch (sortBy.ToLower())
                {
                    case "regno":
                        query = sortOrder?.ToLower() == "desc" ? query.OrderByDescending(v => v.RegNo) : query.OrderBy(v => v.RegNo);
                        break;
                    case "brand":
                         query = sortOrder?.ToLower() == "desc" ? query.OrderByDescending(v => v.Brand.BrandName) : query.OrderBy(v => v.Brand.BrandName);
                        break;
                    case "model":
                        query = sortOrder?.ToLower() == "desc" ? query.OrderByDescending(v => v.Model.ModelName) : query.OrderBy(v => v.Model.ModelName);
                        break;
                    default:
                        query = sortOrder?.ToLower() == "desc" ? query.OrderByDescending(v => v.CreatedAt) : query.OrderBy(v => v.CreatedAt);
                        break;
                }
            }
            else
            {
                query = query.OrderByDescending(v => v.CreatedAt); // Default Sort
            }

            // Pagination (Skip & Take)
            var items = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (items, totalCount);
        }

        // 2. GET VEHICLE BY ID
        public async Task<VehicleMaster?> GetByIdAsync(Guid id)
        {
            return await _context.Vehicles
                .Include(v => v.Brand)
                .Include(v => v.Model)
                .Include(v => v.Dealer)
                .FirstOrDefaultAsync(v => v.VehicleId == id);
        }

        // 2.1 GET VEHICLE BY REGISTRATION NUMBER
        public async Task<VehicleMaster?> GetByRegNoAsync(string regNo)
        {
            return await _context.Vehicles
                .Include(v => v.Brand)
                .Include(v => v.Model)
                .FirstOrDefaultAsync(v => v.RegNo == regNo);
        }

        // 3. ADD (INSERT) VEHICLE
        public async Task AddAsync(VehicleMaster vehicle)
        {
            await _context.Vehicles.AddAsync(vehicle);
            await _context.SaveChangesAsync();
        }

        // 4. UPDATE VEHICLE
        public async Task UpdateAsync(VehicleMaster vehicle)
        {
            _context.Vehicles.Update(vehicle);
            await _context.SaveChangesAsync();
        }

        // 5. DELETE VEHICLE
        public async Task DeleteAsync(Guid id)
        {
            var vehicle = await _context.Vehicles.FindAsync(id);
            if (vehicle != null)
            {
                _context.Vehicles.Remove(vehicle);
                await _context.SaveChangesAsync();
            }
        }

        // 5.1 GET DASHBOARD STATISTICS (OPTIMIZED)
        public async Task<(int TotalCount, int ActiveCount, List<VehicleMaster> RecentVehicles)> GetDashboardStatsAsync()
        {
            // Use database aggregation for counts (efficient)
            var totalCount = await _context.Vehicles.CountAsync();
            var activeCount = await _context.Vehicles.CountAsync(v => v.IsActive);
            
            // Only fetch the 5 most recent vehicles
            var recentVehicles = await _context.Vehicles
                .Include(v => v.Brand)
                .Include(v => v.Model)
                .OrderByDescending(v => v.CreatedAt)
                .Take(5)
                .ToListAsync();

            return (totalCount, activeCount, recentVehicles);
        }

        // 6. GET ALL VEHICLES VIA STORED PROCEDURE
        public async Task<List<VehicleMaster>> GetAllViaStoredProcAsync()
        {
            // Note: Since the SP returns columns that map to VehicleMaster, 
            // EF Core can map the results directly.
            // However, .Include() (joins) won't work automatically on SP results 
            // unless the SP returns the exact structure or you handle mapping manually.
            return await _context.Vehicles
                .FromSqlRaw("EXEC sp_GetAllVehicles")
                .ToListAsync();
        }

        // 7. CREATE VEHICLE VIA STORED PROCEDURE
        public async Task CreateViaStoredProcAsync(VehicleMaster vehicle)
        {
            var parameters = new[]
            {
                new SqlParameter("@VehicleId", vehicle.VehicleId),
                new SqlParameter("@RegNo", vehicle.RegNo ?? (object)DBNull.Value),
                new SqlParameter("@ChassisNumber", vehicle.ChassisNumber ?? (object)DBNull.Value),
                new SqlParameter("@BrandId", vehicle.BrandId),
                new SqlParameter("@ModelId", vehicle.ModelId),
                new SqlParameter("@YearOfManufacture", vehicle.YearOfManufacture),
                new SqlParameter("@VehicleType", (int)vehicle.VehicleType),
                new SqlParameter("@FuelType", (int)vehicle.FuelType),
                new SqlParameter("@Transmission", (int)vehicle.Transmission),
                new SqlParameter("@SeatingCapacity", vehicle.SeatingCapacity),
                new SqlParameter("@VehicleColour", vehicle.VehicleColour ?? (object)DBNull.Value),
                new SqlParameter("@EngineNumber", vehicle.EngineNumber ?? (object)DBNull.Value),
                new SqlParameter("@InsurancePolicyNumber", vehicle.InsurancePolicyNumber ?? (object)DBNull.Value),
                new SqlParameter("@InsurancePolicyExpiryDate", vehicle.InsurancePolicyExpiryDate ?? (object)DBNull.Value),
                new SqlParameter("@RcExpiryDate", vehicle.RcExpiryDate ?? (object)DBNull.Value),
                new SqlParameter("@FitnessCertificateExpiryDate", vehicle.FitnessCertificateExpiryDate ?? (object)DBNull.Value),
                new SqlParameter("@IsActive", vehicle.IsActive),
                new SqlParameter("@CurrentStatus", (int)vehicle.CurrentStatus),
                new SqlParameter("@CreatedAt", DateTime.UtcNow)
            };

            await _context.Database.ExecuteSqlRawAsync(
                "EXEC sp_CreateVehicle @VehicleId, @RegNo, @ChassisNumber, @BrandId, @ModelId, @YearOfManufacture, @VehicleType, @FuelType, @Transmission, @SeatingCapacity, @VehicleColour, @EngineNumber, @InsurancePolicyNumber, @InsurancePolicyExpiryDate, @RcExpiryDate, @FitnessCertificateExpiryDate, @IsActive, @CurrentStatus, @CreatedAt", 
                parameters);
        }
    }
}