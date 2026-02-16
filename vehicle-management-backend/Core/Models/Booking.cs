using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace vehicle_management_backend.Core.Models
{
    public class Booking
    {
        [Key]
        public Guid BookingId { get; set; }

        [Required]
        public Guid VehicleId { get; set; }
        [ForeignKey("VehicleId")]
        public VehicleMaster? Vehicle { get; set; }

        [Required]
        public int DealerId { get; set; }
        [ForeignKey("DealerId")]
        public Dealer? Dealer { get; set; }

        [Required]
        public int CustomerId { get; set; }
        [ForeignKey("CustomerId")]
        public Customer? Customer { get; set; }

        [Required]
        public DateTime StartDate { get; set; }

        [Required]
        public DateTime EndDate { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; }

        public string PaymentMethod { get; set; } = string.Empty; // Cash, Card, UPI
        public string PaymentStatus { get; set; } = "Pending"; // Pending, Paid, Failed

        public int Status { get; set; } = 0; // 0: Pending, 1: Confirmed, 2: Completed, 3: Cancelled

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
