using Microsoft.AspNetCore.Mvc;
using System.Text.RegularExpressions;
using vehicle_management_backend.Application.Services.Interfaces;
using vehicle_management_backend.Core.DTOs;
using vehicle_management_backend.Core.Models;
using vehicle_management_backend.Infrastructure.Data;
using vehicle_management_backend.Infrastructure.Repositories.Interfaces;
using vehicle_management_backend.Core.Enums;
namespace vehicle_management_backend.Controllers
{
    [ApiController]
    [Route("api/vehicles")]
    public class VehicleController : ControllerBase
    {
        private readonly IVehicleService _vehicleService;
        private readonly IBrandService _brandService;
        private readonly IModelService _modelService;
        private readonly IBookingRepository _bookingRepository;
        private readonly AppDbContext _context;
        public VehicleController(IVehicleService vehicleService, IBrandService brandService, IModelService modelService, IBookingRepository bookingRepository, AppDbContext context)
        {
            _vehicleService = vehicleService;
            _brandService = brandService;
            _modelService = modelService;
            _bookingRepository = bookingRepository;
            _context = context;
        }
        [HttpGet("test")]
        public IActionResult Test()
        {
            return Ok(new { message = "API is working", timestamp = DateTime.Now });
        }
        [HttpPost("simple")]
        public IActionResult CreateSimple()
        {
            return Ok(new { message = "Simple endpoint works" });
        }
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateVehicleWithoutNameDTO dto)
        {
            try
            {
                if (dto == null)
                {
                    return BadRequest("Request body is null");
                }
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                // Validate Brand exists
                var brand = await _brandService.GetBrandByIdAsync(dto.BrandId);
                if (brand == null)
                {
                    return BadRequest(new { 
                        error = "Invalid Brand", 
                        message = $"Brand with ID '{dto.BrandId}' does not exist in the database. Please select a valid brand." 
                    });
                }

                // Validate Model exists
                var model = await _modelService.GetModelByIdAsync(dto.ModelId);
                if (model == null)
                {
                    return BadRequest(new { 
                        error = "Invalid Model", 
                        message = $"Model with ID '{dto.ModelId}' does not exist in the database. Please select a valid model." 
                    });
                }

                // Validate Model belongs to the selected Brand
                if (model.BrandId != dto.BrandId)
                {
                    return BadRequest(new { 
                        error = "Model-Brand Mismatch", 
                        message = $"The selected model '{model.ModelName}' does not belong to the selected brand '{brand.BrandName}'. Please select a valid model for this brand." 
                    });
                }

                var vehicle = new VehicleMaster
                {
                    VehicleId = Guid.NewGuid(),
                    RegNo = dto.RegNo ?? string.Empty,
                    DealerId = dto.DealerId,
                    ChassisNumber = dto.ChassisNumber ?? string.Empty,
                    BrandId = dto.BrandId,
                    ModelId = dto.ModelId,
                    YearOfManufacture = dto.YearOfManufacture,
                    VehicleType = dto.VehicleType,
                    FuelType = dto.FuelType,
                    Transmission = dto.Transmission,
                    SeatingCapacity = dto.SeatingCapacity,
                    VehicleColour = dto.VehicleColour,
                    OneDayRate = dto.OneDayRate,
                    EngineNumber = dto.EngineNumber,
                    InsurancePolicyNumber = dto.InsurancePolicyNumber,
                    InsurancePolicyExpiryDate = dto.InsurancePolicyExpiryDate,
                    RcExpiryDate = dto.RcExpiryDate,
                    FitnessCertificateExpiryDate = dto.FitnessCertificateExpiryDate,
                    IsActive = dto.IsActive,
                    CurrentStatus = dto.CurrentStatus
                };
                await _vehicleService.CreateAsync(vehicle);

                return Ok(new { vehicleId = vehicle.VehicleId, message = "Vehicle saved successfully" });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                return StatusCode(500, new { error = ex.Message, innerException = ex.InnerException?.Message });
            }
        }
        [HttpGet]
        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] string? search, [FromQuery] string? brand, [FromQuery] int? status, [FromQuery] bool? isActive, [FromQuery] string? sortBy, [FromQuery] string? sortOrder, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            try
            {
               // Call Optimized Service Method
               var (vehicles, totalCount) = await _vehicleService.GetVehiclesAsync(search, brand, status, isActive, sortBy, sortOrder, page, pageSize);

               var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);

               var dtos = vehicles.Select(v => new VehicleDTO
                    {
                        VehicleId = v.VehicleId,
                        RegNo = v.RegNo,
                        ChassisNumber = v.ChassisNumber,
                        BrandId = v.BrandId,
                        ModelId = v.ModelId,
                        BrandName = v.Brand?.BrandName ?? "Unknown",
                        ModelName = v.Model?.ModelName ?? "Unknown",
                        DealerId = v.DealerId,
                        DealerName = v.Dealer?.Name ?? "Unknown",
                        VehicleType = v.VehicleType.ToString(),
                        FuelType = v.FuelType.ToString(),
                        Transmission = v.Transmission.ToString(),
                        SeatingCapacity = v.SeatingCapacity,
                        VehicleColour = v.VehicleColour,
                        YearOfManufacture = v.YearOfManufacture,
                        OneDayRate = v.OneDayRate,
                        EngineNumber = v.EngineNumber,
                        InsurancePolicyNumber = v.InsurancePolicyNumber,
                        InsurancePolicyExpiryDate = v.InsurancePolicyExpiryDate,
                        RcExpiryDate = v.RcExpiryDate,
                        FitnessCertificateExpiryDate = v.FitnessCertificateExpiryDate,
                        IsActive = v.IsActive,
                        CurrentStatus = v.CurrentStatus
                    }).ToList();

                return Ok(new
                {
                    totalCount,
                    page,
                    data = dtos,
                    totalPages,
                    totalRecords = totalCount,
                    pageSize
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetAll: {ex.Message}");
                // Return empty list on error
                return Ok(new
                {
                    totalCount = 0,
                    page = 1,
                    data = new List<VehicleDTO>(),
                    totalPages = 1,
                    totalRecords = 0,
                    pageSize = 10
                });
            }
        }
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(string id)
        {
            try
            {
                VehicleMaster? vehicle = null;
                if (Guid.TryParse(id, out Guid vehicleId))
                {
                    vehicle = await _vehicleService.GetByIdAsync(vehicleId);
                }
                else
                {
                    // Use efficient RegNo lookup instead of loading all vehicles
                    vehicle = await _vehicleService.GetByRegNoAsync(id);
                }
                if (vehicle == null) return NotFound();
                var brands = await _brandService.GetBrandsAsync();
                var models = await _modelService.GetModelsAsync();
                var vehicleBrand = brands.FirstOrDefault(b => b.BrandId == vehicle.BrandId);
                var vehicleModel = models.FirstOrDefault(m => m.ModelId == vehicle.ModelId);
                var dto = new VehicleDTO
                {
                    VehicleId = vehicle.VehicleId,
                    RegNo = vehicle.RegNo,
                    ChassisNumber = vehicle.ChassisNumber,
                    BrandId = vehicle.BrandId,
                    ModelId = vehicle.ModelId,
                    BrandName = vehicleBrand?.BrandName ?? "Unknown",
                    ModelName = vehicleModel?.ModelName ?? "Unknown",
                    DealerId = vehicle.DealerId,
                    DealerName = vehicle.Dealer?.Name ?? "Unknown",
                    VehicleType = vehicle.VehicleType.ToString(),
                    FuelType = vehicle.FuelType.ToString(),
                    Transmission = vehicle.Transmission.ToString(),
                    SeatingCapacity = vehicle.SeatingCapacity,
                    VehicleColour = vehicle.VehicleColour,
                    YearOfManufacture = vehicle.YearOfManufacture,
                    OneDayRate = vehicle.OneDayRate,
                    EngineNumber = vehicle.EngineNumber,
                    InsurancePolicyNumber = vehicle.InsurancePolicyNumber,
                    InsurancePolicyExpiryDate = vehicle.InsurancePolicyExpiryDate,
                    RcExpiryDate = vehicle.RcExpiryDate,
                    FitnessCertificateExpiryDate = vehicle.FitnessCertificateExpiryDate,
                    IsActive = vehicle.IsActive,
                    CurrentStatus = vehicle.CurrentStatus
                };
                return Ok(dto);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetById: {ex.Message}");
                return StatusCode(500, new { error = ex.Message });
            }
        }
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(string id, [FromBody] UpdateVehicleDTO dto)
        {
            try
            {
                Console.WriteLine($"Update request received for ID: {id}");
                Console.WriteLine($"DTO: {System.Text.Json.JsonSerializer.Serialize(dto)}");
                
                VehicleMaster? vehicle = null;
                if (Guid.TryParse(id, out Guid vehicleId))
                {
                    vehicle = await _vehicleService.GetByIdAsync(vehicleId);
                }
                else
                {
                    // Use efficient RegNo lookup instead of loading all vehicles
                    vehicle = await _vehicleService.GetByRegNoAsync(id);
                }
                
                if (vehicle == null)
                {
                    Console.WriteLine($"Vehicle not found for ID: {id}");
                    return NotFound(new { message = "Vehicle not found" });
                }

                // Validate Brand exists
                var brand = await _brandService.GetBrandByIdAsync(dto.BrandId);
                if (brand == null)
                {
                    return BadRequest(new { 
                        error = "Invalid Brand", 
                        message = $"Brand with ID '{dto.BrandId}' does not exist in the database. Please select a valid brand." 
                    });
                }

                // Validate Model exists
                var model = await _modelService.GetModelByIdAsync(dto.ModelId);
                if (model == null)
                {
                    return BadRequest(new { 
                        error = "Invalid Model", 
                        message = $"Model with ID '{dto.ModelId}' does not exist in the database. Please select a valid model." 
                    });
                }

                // Validate Model belongs to the selected Brand
                if (model.BrandId != dto.BrandId)
                {
                    return BadRequest(new { 
                        error = "Model-Brand Mismatch", 
                        message = $"The selected model '{model.ModelName}' does not belong to the selected brand '{brand.BrandName}'. Please select a valid model for this brand." 
                    });
                }

                vehicle.RegNo = dto.RegNo;
                vehicle.DealerId = dto.DealerId;
                vehicle.ChassisNumber = dto.ChassisNumber;
                vehicle.BrandId = dto.BrandId;
                vehicle.ModelId = dto.ModelId;
                vehicle.YearOfManufacture = dto.YearOfManufacture;
                vehicle.VehicleType = dto.VehicleType;
                vehicle.FuelType = dto.FuelType;
                vehicle.Transmission = dto.Transmission;
                vehicle.SeatingCapacity = dto.SeatingCapacity;
                vehicle.VehicleColour = dto.VehicleColour;
                vehicle.OneDayRate = dto.OneDayRate;
                vehicle.EngineNumber = dto.EngineNumber;
                vehicle.InsurancePolicyNumber = dto.InsurancePolicyNumber;
                vehicle.InsurancePolicyExpiryDate = dto.InsurancePolicyExpiryDate;
                vehicle.RcExpiryDate = dto.RcExpiryDate;
                vehicle.FitnessCertificateExpiryDate = dto.FitnessCertificateExpiryDate;
                vehicle.IsActive = dto.IsActive;
                vehicle.CurrentStatus = dto.CurrentStatus; 
                
                await _vehicleService.UpdateAsync(vehicle);
                
                Console.WriteLine($"Vehicle updated successfully: {vehicle.VehicleId}");
                return Ok(new { message = "Vehicle updated successfully", vehicleId = vehicle.VehicleId });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in Update: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                return StatusCode(500, new { error = ex.Message, details = ex.InnerException?.Message });
            }
        }
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(string id)
        {
            try
            {
                // 1. Define variables to hold data for logging
                string regNo = "Unknown";
                Guid vId = Guid.Empty;

                // 2. Find the vehicle first to get its RegNo (before deleting it)
                if (Guid.TryParse(id, out Guid vehicleId))
                {
                    var vehicle = await _vehicleService.GetByIdAsync(vehicleId);
                    if (vehicle == null) return NotFound();

                    regNo = vehicle.RegNo; // Capture RegNo
                    vId = vehicleId;
                }
                else
                {
                    // Use efficient RegNo lookup instead of loading all vehicles
                    var vehicle = await _vehicleService.GetByRegNoAsync(id);
                    if (vehicle == null) return NotFound();

                    regNo = vehicle.RegNo; // Capture RegNo
                    vId = vehicle.VehicleId;
                }

                // 3. Perform the Delete
                await _vehicleService.DeleteAsync(vId);

                // 5. Return success response
                return NoContent();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in Delete: {ex.Message}");
                return StatusCode(500, new { error = ex.Message });
            }
        }
        [HttpPut("set-maintenance/{id}")]
        public async Task<IActionResult> SetMaintenanceStatus(Guid id, [FromQuery] bool inMaintenance)
        {
            VehicleMaster? vehicle = null;
            if (Guid.TryParse(id.ToString(), out Guid vehicleId))
            {
                vehicle = await _vehicleService.GetByIdAsync(vehicleId);
            }
            // If checking by RegNo is needed, handled by TryParse failure, but method signature expects Guid. 
            // The route {id} might receive a string, but the action argument is Guid. 
            // If the frontend sends a GUID string, it binds correctly.
            
            if (vehicle == null) return NotFound();

            if (inMaintenance)
            {
                // Check if vehicle has active bookings before sending to maintenance
                bool hasActiveBooking = await _bookingRepository.IsVehicleBusyNow(id); 
                if (hasActiveBooking) 
                {
                    return BadRequest("Cannot move to maintenance: Vehicle is currently rented.");
                }
                vehicle.CurrentStatus = (int)VehicleStatus.Inmaintance;
            }
            else
            {
                // Bringing back from maintenance
                vehicle.CurrentStatus = (int)VehicleStatus.Available;
            }

            await _vehicleService.UpdateAsync(vehicle);
            return Ok(new { Message = $"Vehicle status updated to {(inMaintenance ? "In Maintenance" : "Available")}" });
        }

        [HttpGet("dashboard")]
        public async Task<IActionResult> GetDashboardData()
        {
            // Use optimized dashboard statistics query
            var (totalCount, activeCount, recentVehicles) = await _vehicleService.GetDashboardStatsAsync();
            
            var dashboardData = new
            {
                totalVehicles = totalCount,
                activeVehicles = activeCount,
                recentVehicles = recentVehicles.Select(v => new
                    {
                        vehicleId = v.VehicleId,
                        regNo = v.RegNo,
                        brandId = v.BrandId,
                        modelId = v.ModelId,
                        yearOfManufacture = v.YearOfManufacture,
                        isActive = v.IsActive
                    }).ToList() // Materialize to List for JSON serialization
            };
            return Ok(dashboardData);
        }

        // STORED PROCEDURE ENDPOINTS
        
        [HttpGet("list-sp")]
        public async Task<IActionResult> GetAllViaSP()
        {
            try
            {
                var vehicles = await _vehicleService.GetAllSPAsync();
                var brands = await _brandService.GetBrandsAsync();
                var models = await _modelService.GetModelsAsync();

                if (vehicles == null)
                {
                    vehicles = new List<VehicleMaster>();
                }

                var dtos = vehicles.Select(v =>
                {
                    var vehicleBrand = brands.FirstOrDefault(b => b.BrandId == v.BrandId);
                    var vehicleModel = models.FirstOrDefault(m => m.ModelId == v.ModelId);
                    return new VehicleDTO
                    {
                        VehicleId = v.VehicleId,
                        RegNo = v.RegNo,
                        ChassisNumber = v.ChassisNumber,
                        BrandId = v.BrandId,
                        ModelId = v.ModelId,
                        BrandName = vehicleBrand?.BrandName ?? "Unknown",
                        ModelName = vehicleModel?.ModelName ?? "Unknown",
                        DealerId = v.DealerId,
                        DealerName = v.Dealer?.Name ?? "Unknown",
                        VehicleType = v.VehicleType.ToString(),
                        FuelType = v.FuelType.ToString(),
                        Transmission = v.Transmission.ToString(),
                        SeatingCapacity = v.SeatingCapacity,
                        VehicleColour = v.VehicleColour,
                        YearOfManufacture = v.YearOfManufacture,
                        OneDayRate = v.OneDayRate,
                        EngineNumber = v.EngineNumber,
                        InsurancePolicyNumber = v.InsurancePolicyNumber,
                        InsurancePolicyExpiryDate = v.InsurancePolicyExpiryDate,
                        RcExpiryDate = v.RcExpiryDate,
                        FitnessCertificateExpiryDate = v.FitnessCertificateExpiryDate,
                        IsActive = v.IsActive,
                        CurrentStatus = v.CurrentStatus
                    };
                }).ToList();

                return Ok(dtos);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetAllViaSP: {ex.Message}");
                return StatusCode(500, new { error = ex.Message, innerException = ex.InnerException?.Message });
            }
        }

        [HttpPost("create-sp")]
        public async Task<IActionResult> CreateViaSP([FromBody] CreateVehicleWithoutNameDTO dto)
        {
            try
            {
                if (dto == null)
                {
                    return BadRequest("Request body is null");
                }
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                // Validate Brand exists
                var brand = await _brandService.GetBrandByIdAsync(dto.BrandId);
                if (brand == null)
                {
                    return BadRequest(new { 
                        error = "Invalid Brand", 
                        message = $"Brand with ID '{dto.BrandId}' does not exist in the database. Please select a valid brand." 
                    });
                }

                // Validate Model exists
                var model = await _modelService.GetModelByIdAsync(dto.ModelId);
                if (model == null)
                {
                    return BadRequest(new { 
                        error = "Invalid Model", 
                        message = $"Model with ID '{dto.ModelId}' does not exist in the database. Please select a valid model." 
                    });
                }

                // Validate Model belongs to the selected Brand
                if (model.BrandId != dto.BrandId)
                {
                    return BadRequest(new { 
                        error = "Model-Brand Mismatch", 
                        message = $"The selected model '{model.ModelName}' does not belong to the selected brand '{brand.BrandName}'. Please select a valid model for this brand." 
                    });
                }

                var vehicle = new VehicleMaster
                {
                    VehicleId = Guid.NewGuid(),
                    RegNo = dto.RegNo ?? string.Empty,
                    ChassisNumber = dto.ChassisNumber ?? string.Empty,
                    BrandId = dto.BrandId,
                    ModelId = dto.ModelId,
                    YearOfManufacture = dto.YearOfManufacture,
                    VehicleType = dto.VehicleType,
                    FuelType = dto.FuelType,
                    Transmission = dto.Transmission,
                    SeatingCapacity = dto.SeatingCapacity,
                    VehicleColour = dto.VehicleColour,
                    OneDayRate = dto.OneDayRate,
                    EngineNumber = dto.EngineNumber,
                    InsurancePolicyNumber = dto.InsurancePolicyNumber,
                    InsurancePolicyExpiryDate = dto.InsurancePolicyExpiryDate,
                    RcExpiryDate = dto.RcExpiryDate,
                    FitnessCertificateExpiryDate = dto.FitnessCertificateExpiryDate,
                    IsActive = dto.IsActive,
                    CurrentStatus = dto.CurrentStatus
                };

                // Call the SP version of Create
                await _vehicleService.CreateSPAsync(vehicle);

                _context.ActivityLogs.Add(new ActivityLog
                {
                    Message = $"New vehicle registered via SP: {vehicle.RegNo}",
                    Type = "success",
                    CreatedAt = DateTime.UtcNow
                });
                await _context.SaveChangesAsync();

                return Ok(new { vehicleId = vehicle.VehicleId, message = "Vehicle created via Stored Procedure" });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in CreateViaSP: {ex.Message}");
                return StatusCode(500, new { error = ex.Message, innerException = ex.InnerException?.Message });
            }
        }
    }
}