using vehicle_management_backend.Core.DTOs;

namespace vehicle_management_backend.Application.Services.Interfaces
{
    public interface IBookingService
    {
        Task<List<BookingDTO>> GetAllBookingsAsync();
        Task<BookingDTO?> GetBookingByIdAsync(Guid id);
        Task<BookingDTO> CreateBookingAsync(CreateBookingDTO createBookingDTO);
        Task<BookingDTO?> UpdateBookingAsync(Guid id, UpdateBookingDTO updateBookingDTO);
        Task<bool> DeleteBookingAsync(Guid id);
    }
}
