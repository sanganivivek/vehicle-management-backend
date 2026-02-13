using System.ComponentModel.DataAnnotations;

namespace vehicle_management_backend.Core.DTOs
{
    public class UpdateVehicleDTO
    {
        public Guid VehicleId { get; set; }
        public int? DealerId { get; set; }
        public string RegNo { get; set; } = string.Empty;
        public string ChassisNumber { get; set; } = string.Empty;
        
        public Guid BrandId { get; set; }
        public Guid ModelId { get; set; }

        public vehicle_management_backend.Core.Enums.VehicleType VehicleType { get; set; }
        public vehicle_management_backend.Core.Enums.FuelType FuelType { get; set; }
        public vehicle_management_backend.Core.Enums.Transmission Transmission { get; set; }
        
        public int SeatingCapacity { get; set; }
        public string? VehicleColour { get; set; }
        public int YearOfManufacture { get; set; }
        public decimal OneDayRate { get; set; }
        
        public string? EngineNumber { get; set; }
        public string? InsurancePolicyNumber { get; set; }
        public DateTime? InsurancePolicyExpiryDate { get; set; }
        public DateTime? RcExpiryDate { get; set; }
        public DateTime? FitnessCertificateExpiryDate { get; set; }

        public bool IsActive { get; set; }
        public int CurrentStatus { get; set; } = 0;
    }
}