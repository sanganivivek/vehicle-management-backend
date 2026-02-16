using vehicle_management_backend.Application.Services.Interfaces;
using vehicle_management_backend.Core.DTOs;
using vehicle_management_backend.Core.Models;
using vehicle_management_backend.Infrastructure.Repositories.Interfaces;

namespace vehicle_management_backend.Application.Services.Implementations
{
    public class BookingService : IBookingService
    {
        private readonly IBookingRepository _bookingRepository;
        private readonly IVehicleRepository _vehicleRepository;
        private readonly IDealerRepository _dealerRepository; 
        private readonly ICustomerRepository _customerRepository;
        // Assuming repositories for Dealer and Customer exist and are needed for validation, 
        // though strictly foreign key constraints might handle existence. 
        // Better to validate to return friendly errors.

        public BookingService(
            IBookingRepository bookingRepository,
            IVehicleRepository vehicleRepository,
            IDealerRepository dealerRepository,
            ICustomerRepository customerRepository)
        {
            _bookingRepository = bookingRepository;
            _vehicleRepository = vehicleRepository;
            _dealerRepository = dealerRepository;
            _customerRepository = customerRepository;
        }

        public async Task<List<BookingDTO>> GetAllBookingsAsync()
        {
            var bookings = await _bookingRepository.GetAllAsync();
            return bookings.Select(MapToDTO).ToList();
        }

        public async Task<BookingDTO?> GetBookingByIdAsync(Guid id)
        {
            var booking = await _bookingRepository.GetByIdAsync(id);
            return booking == null ? null : MapToDTO(booking);
        }

        public async Task<BookingDTO> CreateBookingAsync(CreateBookingDTO dto)
        {
            // 1. Validate Vehicle
            var vehicle = await _vehicleRepository.GetByIdAsync(dto.VehicleId);
            if (vehicle == null)
            {
                throw new Exception("Vehicle not found.");
            }
            if (!vehicle.IsActive)
            {
                throw new Exception("Vehicle is not active and cannot be booked.");
            }

            // 2. Validate Dates
            if (dto.StartDate >= dto.EndDate)
            {
                throw new Exception("End date must be after start date.");
            }
            if (dto.StartDate.Date < DateTime.UtcNow.Date)
            {
                throw new Exception("Start date cannot be in the past.");
            }

            // 3. Check Availability
            bool isAvailable = await _bookingRepository.IsVehicleAvailableAsync(dto.VehicleId, dto.StartDate, dto.EndDate);
            if (!isAvailable)
            {
                throw new Exception("Vehicle is already booked for the selected dates.");
            }

            // 4. Calculate Amount
            var days = (dto.EndDate - dto.StartDate).Days;
            if (days == 0) days = 1; // Minimum 1 day charge
            var amount = days * vehicle.OneDayRate;

            // 5. Create Entity
            var booking = new Booking
            {
                BookingId = Guid.NewGuid(),
                VehicleId = dto.VehicleId,
                DealerId = dto.DealerId,
                CustomerId = dto.CustomerId,
                StartDate = dto.StartDate,
                EndDate = dto.EndDate,
                Amount = amount,
                PaymentMethod = dto.PaymentMethod,
                PaymentStatus = dto.PaymentStatus,
                Status = dto.Status,
                CreatedAt = DateTime.UtcNow
            };

            await _bookingRepository.AddAsync(booking);

            // Return DTO with loaded relations (might need to fetch again or manually map knowns)
            // For simplicity, returning what we have, knowing relations might be null in local 'booking' object
            // To be safe and return full names, we should fetch it or manually attach if context allows.
            // Let's manually populate what we know to avoid extra DB call, 
            // but Dealer/Customer names are not in DTO input. 
            // So we ideally fetch the created booking. Use GetByIdAsync.
            var createdBooking = await _bookingRepository.GetByIdAsync(booking.BookingId);
            return MapToDTO(createdBooking!);
        }

        public async Task<BookingDTO?> UpdateBookingAsync(Guid id, UpdateBookingDTO dto)
        {
            var booking = await _bookingRepository.GetByIdAsync(id);
            if (booking == null) return null;

            // Update Validation
             if (dto.VehicleId != Guid.Empty && dto.VehicleId != booking.VehicleId)
            {
                 // Logic to change vehicle if needed
                 // Validate new vehicle...
                 // For now, let's assume vehicle change needs availability check
                 var vehicle = await _vehicleRepository.GetByIdAsync(dto.VehicleId);
                 if (vehicle != null && !vehicle.IsActive) throw new Exception("New vehicle is inactive.");
                 
                 // Update ID
                 booking.VehicleId = dto.VehicleId;
            }

            // Update Dates & Recalculate Amount if changed
            bool datesChanged = false;
            if (dto.StartDate != default && dto.StartDate != booking.StartDate)
            {
                booking.StartDate = dto.StartDate;
                datesChanged = true;
            }
            if (dto.EndDate != default && dto.EndDate != booking.EndDate)
            {
                booking.EndDate = dto.EndDate;
                datesChanged = true;
            }

            if (datesChanged)
            {
                 if (booking.StartDate >= booking.EndDate) throw new Exception("End date must be after start date.");
                 
                 // Check availability logic again...
                 bool isAvailable = await _bookingRepository.IsVehicleAvailableAsync(
                     booking.VehicleId, booking.StartDate, booking.EndDate, booking.BookingId);
                 if (!isAvailable) throw new Exception("Vehicle not available for new dates.");

                 // Recalculate amount
                 var vehicle = await _vehicleRepository.GetByIdAsync(booking.VehicleId);
                 if (vehicle != null)
                 {
                     var days = (booking.EndDate - booking.StartDate).Days;
                     if (days == 0) days = 1;
                     booking.Amount = days * vehicle.OneDayRate;
                 }
            }

            // Standard updates
            if (dto.DealerId != 0) booking.DealerId = dto.DealerId;
            if (dto.CustomerId != 0) booking.CustomerId = dto.CustomerId;
            if (!string.IsNullOrEmpty(dto.PaymentMethod)) booking.PaymentMethod = dto.PaymentMethod;
            if (!string.IsNullOrEmpty(dto.PaymentStatus)) booking.PaymentStatus = dto.PaymentStatus;
            
            // Status update (Allow 0 if intentionally setting to Pending, but DTO might send 0 as default? 
            // UpdateDTO should probably use nullable int? for status to distinguish.
            // But let's assume checking against current status or if DTO has specific value)
            // For now, strict mapping:
            booking.Status = dto.BookingStatus;

            await _bookingRepository.UpdateAsync(booking);
            
            var updatedBooking = await _bookingRepository.GetByIdAsync(id);
            return MapToDTO(updatedBooking!);
        }

        public async Task<bool> DeleteBookingAsync(Guid id)
        {
            var booking = await _bookingRepository.GetByIdAsync(id);
            if (booking == null) return false;

            await _bookingRepository.DeleteAsync(id);
            return true;
        }

        private BookingDTO MapToDTO(Booking booking)
        {
            return new BookingDTO
            {
                BookingId = booking.BookingId,
                VehicleId = booking.VehicleId,
                VehicleName = $"{booking.Vehicle?.Brand?.BrandName} {booking.Vehicle?.Model?.ModelName}", // Construct composite name
                VehicleRegNo = booking.Vehicle?.RegNo ?? "",
                DealerId = booking.DealerId,
                DealerName = booking.Dealer?.Name ?? "",
                CustomerId = booking.CustomerId,
                CustomerName = booking.Customer?.Name ?? "",
                StartDate = booking.StartDate,
                EndDate = booking.EndDate,
                Amount = booking.Amount,
                PaymentMethod = booking.PaymentMethod,
                PaymentStatus = booking.PaymentStatus,
                Status = booking.Status,
                StatusName = GetStatusName(booking.Status),
                CreatedAt = booking.CreatedAt
            };
        }

        private string GetStatusName(int status)
        {
            return status switch
            {
                0 => "Pending",
                1 => "Confirmed",
                2 => "Completed",
                3 => "Cancelled",
                _ => "Unknown"
            };
        }
    }
}
