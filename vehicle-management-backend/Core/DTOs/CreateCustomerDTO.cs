using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace vehicle_management_backend.Core.DTOs
{
    public class CreateCustomerDTO
    {
        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("Email")]
        public string Email { get; set; }

        [JsonPropertyName("contactNo")]
        public string ContactNo { get; set; }

        [JsonPropertyName("gender")]
        public string Gender { get; set; }

        [JsonPropertyName("dateOfBirth")]
        public DateTime DateOfBirth { get; set; }

        [JsonPropertyName("city")]
        public string City { get; set; }

        [JsonPropertyName("address")]
        public string Address { get; set; }

        [JsonPropertyName("status")]
        public string Status { get; set; } // Added field
    }
}
