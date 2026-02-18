using vehicle_management_backend.Core.DTOs;

namespace vehicle_management_backend.Core.DTOs
{
    public class BrandDTO
    {
        public Guid? BrandId { get; set; }
        public string BrandName { get; set; }
        public string BrandCode { get; set; }
        public bool IsActive { get; set;  }
    }
}

namespace vehicle_management_backend
{
    public class CreateBrandDTO : BrandDTO
    {
        public string BrandName { get; set; }
        public string BrandCode { get; set; }
        public bool IsActive { get; set; }
    }
}