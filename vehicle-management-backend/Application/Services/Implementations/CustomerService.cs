using vehicle_management_backend.Application.Services.Interfaces;
using vehicle_management_backend.Core.DTOs;
using vehicle_management_backend.Core.Models;
using vehicle_management_backend.Infrastructure.Repositories.Interfaces;

namespace vehicle_management_backend.Application.Services.Implementations
{
    public class CustomerService : ICustomerService
    {
        private readonly ICustomerRepository _customerRepository;

        public CustomerService(ICustomerRepository customerRepository)
        {
            _customerRepository = customerRepository;
        }

        public async Task<IEnumerable<Customer>> GetAllDealersAsync()
        {
            return await _customerRepository.GetAllAsync();
        }

        public async Task<Customer> GetCustomerByIdAsync(int id)
        {
            return await _customerRepository.GetByIdAsync(id);
        }

        // FIXED: Renamed from AddDealerAsync to CreateDealerAsync to match Interface
        public async Task<Customer> CreateCustomerAsync(CreateCustomerDTO customerDto)
        {
            var customer = new Customer
            {
                Name = customerDto.Name,
                Email = customerDto.Email,
                ContactNo = customerDto.ContactNo,
                Gender = customerDto.Gender,
                DateOfBirth = customerDto.DateOfBirth,
                City = customerDto.City,
                Address = customerDto.Address,
                Status = customerDto.Status,
                CreatedDate = DateTime.Now
            };

            return await _customerRepository.AddAsync(customer);
        }

        public async Task<Customer> UpdateCustomerAsync(int id, UpdateCustomerDTO customerDto)
        {
            var existingCustomer = await _customerRepository.GetByIdAsync(id);
            if (existingCustomer == null) return null;

            existingCustomer.Name = customerDto.Name;
            existingCustomer.Email = customerDto.Email;
            existingCustomer.ContactNo = customerDto.ContactNo;
            existingCustomer.Gender = customerDto.Gender;
            existingCustomer.DateOfBirth = customerDto.DateOfBirth;
            existingCustomer.City = customerDto.City;
            existingCustomer.Address = customerDto.Address;
            existingCustomer.Status = customerDto.Status;

            await _customerRepository.UpdateAsync(existingCustomer);
            return existingCustomer;
        }

        public async Task<bool> DeleteCustomerAsync(int id)
        {
            var customer = await _customerRepository.GetByIdAsync(id);
            if (customer == null) return false;

            await _customerRepository.DeleteAsync(id);
            return true;
        }
    }
}