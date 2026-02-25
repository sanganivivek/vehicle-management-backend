namespace vehicle_management_backend.Core.DTOs
{
    public class BookingDTO
    {
        public Guid BookingId { get; set; }
        public int BookingNumber { get; set; }
        public string FormattedId => $"#{BookingNumber:D4}"; // e.g. #0001
        public Guid VehicleId { get; set; }
        public string VehicleName { get; set; } = string.Empty;
        public string VehicleModel { get; set; } = string.Empty;
        public string VehicleBrand { get; set; } = string.Empty;
        public string VehicleRegNo { get; set; } = string.Empty;
        
        public int DealerId { get; set; }
        public string DealerName { get; set; } = string.Empty;

        public int CustomerId { get; set; }
        public string CustomerName { get; set; } = string.Empty;

        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public decimal Amount { get; set; }
        
        public string PaymentMethod { get; set; } = string.Empty;
        public string PaymentStatus { get; set; } = string.Empty;
        
        public int Status { get; set; }
        public string StatusName { get; set; } = string.Empty; // e.g., "Confirmed"

        public DateTime CreatedAt { get; set; }
    }
}
