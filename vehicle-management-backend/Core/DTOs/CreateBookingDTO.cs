using System.ComponentModel.DataAnnotations;

namespace vehicle_management_backend.Core.DTOs
{
    public class CreateBookingDTO
    {
        [Required]
        public Guid VehicleId { get; set; }

        [Required]
        public int DealerId { get; set; }

        [Required]
        public int CustomerId { get; set; }

        [Required]
        public DateTime StartDate { get; set; }

        [Required]
        public DateTime EndDate { get; set; }

        public string PaymentMethod { get; set; } = "Cash"; 
        public string PaymentStatus { get; set; } = "Pending";
        public int Status { get; set; } = 0; // Default to Pending
    }
}
