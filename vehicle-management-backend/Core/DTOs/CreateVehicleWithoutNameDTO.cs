using System.ComponentModel.DataAnnotations;
namespace vehicle_management_backend.Core.DTOs
{
    public class CreateVehicleWithoutNameDTO
    {
        [Required]
        public string RegNo { get; set; } = string.Empty;
        [Required]
        public int? DealerId { get; set; }

        [Required]
        public string ChassisNumber { get; set; } = string.Empty;

        [Required]
        public Guid BrandId { get; set; }
        
        [Required]
        public Guid ModelId { get; set; }

        public vehicle_management_backend.Core.Enums.VehicleType VehicleType { get; set; }
        public vehicle_management_backend.Core.Enums.FuelType FuelType { get; set; }
        public vehicle_management_backend.Core.Enums.Transmission Transmission { get; set; }
        
        public int SeatingCapacity { get; set; }
        public string? VehicleColour { get; set; }
        public int YearOfManufacture { get; set; }
        
        [Range(0, double.MaxValue)]
        public decimal OneDayRate { get; set; }
        
        public string? EngineNumber { get; set; }
        public string? InsurancePolicyNumber { get; set; }
        public DateTime? InsurancePolicyExpiryDate { get; set; }
        public DateTime? RcExpiryDate { get; set; }
        public DateTime? FitnessCertificateExpiryDate { get; set; }

        public bool IsActive { get; set; } = true;
        public int CurrentStatus { get; set; } = 0;
    }
}