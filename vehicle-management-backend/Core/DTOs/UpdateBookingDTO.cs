using System.ComponentModel.DataAnnotations;

namespace vehicle_management_backend.Core.DTOs
{
    public class UpdateBookingDTO
    {
        public Guid VehicleId { get; set; }
        public int DealerId { get; set; }
        public int CustomerId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public decimal Amount { get; set; }
        public string PaymentMethod { get; set; } = string.Empty;
        public string PaymentStatus { get; set; } = string.Empty;
        public int BookingStatus { get; set; }
    }
}
