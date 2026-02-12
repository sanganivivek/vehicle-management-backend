using vehicle_management_backend.Application.Services.Interfaces;
using vehicle_management_backend.Core.Models;
using vehicle_management_backend.Infrastructure.Data;

namespace vehicle_management_backend.Application.Services.Implementations
{
    public class ActivityLogService : IActivityLogService
    {
        private readonly AppDbContext _context;

        public ActivityLogService(AppDbContext context)
        {
            _context = context;
        }

        public async Task LogCreateAsync(string message)
        {
            var log = new ActivityLog
            {
                Message = message,
                Type = "success",
                CreatedAt = DateTime.UtcNow
            };

            _context.ActivityLogs.Add(log);
            await _context.SaveChangesAsync();
        }

        public async Task LogUpdateAsync(string message)
        {
            var log = new ActivityLog
            {
                Message = message,
                Type = "info",
                CreatedAt = DateTime.UtcNow
            };

            _context.ActivityLogs.Add(log);
            await _context.SaveChangesAsync();
        }

        public async Task LogDeleteAsync(string message)
        {
            var log = new ActivityLog
            {
                Message = message,
                Type = "warning",
                CreatedAt = DateTime.UtcNow
            };

            _context.ActivityLogs.Add(log);
            await _context.SaveChangesAsync();
        }
    }
}
