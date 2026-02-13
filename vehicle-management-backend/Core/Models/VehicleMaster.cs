using System.ComponentModel.DataAnnotations;
namespace vehicle_management_backend.Core.Models
{
    public class VehicleMaster
    {
        [Key]
        public Guid VehicleId { get; set; }
        
        public string RegNo { get; set; } = string.Empty; // Registration Number
        public string ChassisNumber { get; set; } = string.Empty;

        // Foreign Keys
        public Guid BrandId { get; set; }   
        public Brand? Brand { get; set; }

        public Guid ModelId { get; set; }
        public Model? Model { get; set; }
        public int? DealerId { get; set; }
        public Dealer? Dealer { get; set; }

        // Enums and details
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

        public bool IsActive { get; set; } = true;
        public int CurrentStatus { get; set; } = 0;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}