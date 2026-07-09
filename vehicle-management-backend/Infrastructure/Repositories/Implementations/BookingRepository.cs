using Microsoft.EntityFrameworkCore;
using vehicle_management_backend.Core.Models;
using vehicle_management_backend.Infrastructure.Data;
using vehicle_management_backend.Infrastructure.Repositories.Interfaces;

namespace vehicle_management_backend.Infrastructure.Repositories.Implementations
{
    public class BookingRepository : IBookingRepository
    {
        private readonly AppDbContext _context;

        public BookingRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<Booking>> GetAllAsync()
        {
            return await _context.Bookings
                .Include(b => b.Vehicle)
                    .ThenInclude(v => v.Brand)
                .Include(b => b.Vehicle)
                    .ThenInclude(v => v.Model)
                .Include(b => b.Dealer)
                .Include(b => b.Customer)
                .OrderByDescending(b => b.CreatedAt)
                .ToListAsync();
        }

        public async Task<Booking?> GetByIdAsync(Guid id)
        {
            return await _context.Bookings
                .Include(b => b.Vehicle)
                .Include(b => b.Dealer)
                .Include(b => b.Customer)
                .FirstOrDefaultAsync(b => b.BookingId == id);
        }

        public async Task AddAsync(Booking booking)
        {
            await _context.Bookings.AddAsync(booking);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Booking booking)
        {
            _context.Bookings.Update(booking);
            // BookingNumber is a SQL IDENTITY column — EF Core must NOT try to update it
            _context.Entry(booking).Property(b => b.BookingNumber).IsModified = false;
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Guid id)
        {
            var booking = await _context.Bookings.FindAsync(id);
            if (booking != null)
            {
                _context.Bookings.Remove(booking);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<bool> IsVehicleAvailableAsync(Guid vehicleId, DateTime startDate, DateTime endDate, Guid? excludeBookingId = null)
        {
            // Check for overlapping bookings
            var query = _context.Bookings.Where(b => 
                b.VehicleId == vehicleId && 
                b.Status != 3 && // Not Cancelled
                ((startDate >= b.StartDate && startDate <= b.EndDate) || 
                 (endDate >= b.StartDate && endDate <= b.EndDate) ||
                 (startDate <= b.StartDate && endDate >= b.EndDate)));

            if (excludeBookingId.HasValue)
            {
                query = query.Where(b => b.BookingId != excludeBookingId.Value);
            }

            return !await query.AnyAsync();
        }

        public async Task<bool> IsVehicleBusyNow(Guid vehicleId)
        {
            var currentTime = DateTime.UtcNow;
            return await _context.Bookings.AnyAsync(b => 
                b.VehicleId == vehicleId &&
                b.Status == 1 && // confirmed
                b.StartDate <= currentTime && 
                b.EndDate >= currentTime);
        }
    }
}
