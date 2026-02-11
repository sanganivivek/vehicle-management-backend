using System.ComponentModel.DataAnnotations;
using System.Security;
using System.ComponentModel.DataAnnotations;

namespace vehicle_management_backend.Core.DTOs
{
    public class VehicleDTO
    {
        public Guid VehicleId { get; set; }
        public int? DealerId { get; set; }
        public string? DealerName { get; set; }
        public string RegNo { get; set; }
        public string ChassisNumber { get; set; }

        public Guid BrandId { get; set; }
        public string BrandName { get; set; } = string.Empty;
        
        public Guid ModelId { get; set; }
        public string ModelName { get; set; } = string.Empty;

        public string VehicleType { get; set; }
        public string FuelType { get; set; }
        public string Transmission { get; set; }
     
        public int SeatingCapacity { get; set; }
        public string? VehicleColour { get; set; }
        public int YearOfManufacture { get; set; }
        
        public string? EngineNumber { get; set; }
        public string? InsurancePolicyNumber { get; set; }
        public DateTime? InsurancePolicyExpiryDate { get; set; }
        public DateTime? RcExpiryDate { get; set; }
        public DateTime? FitnessCertificateExpiryDate { get; set; }

        public bool IsActive { get; set; }
        public int CurrentStatus { get; set; }
    }
}