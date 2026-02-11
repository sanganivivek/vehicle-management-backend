using vehicle_management_backend.Core.DTOs;
using vehicle_management_backend.Core.Models;

namespace vehicle_management_backend.Application.Services.Interfaces
{
    public interface ICustomerService
    {
        Task<IEnumerable<Customer>> GetAllCustomerAsync();

        Task<Customer> GetCustomerByIdAsync(int id);

        // You requested 'CreateDealerAsync' here
        Task<Customer> CreateCustomerAsync(CreateCustomerDTO customerDto);

        Task<Customer> UpdateCustomerAsync(int id, UpdateCustomerDTO customerDto);

        Task<bool> DeleteCustomerAsync(int id);
    }
}