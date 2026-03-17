using SmartBookingSystem.Data;
using SmartBookingSystem.Models;

namespace SmartBookingSystem.Services
{
    public interface IAuditService
    {
        Task LogAsync(string? userId, string action, string entityName, string? entityId = null, string? details = null);
    }

    public class AuditService : IAuditService
    {
        private readonly ApplicationDbContext _context;

        public AuditService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task LogAsync(string? userId, string action, string entityName, string? entityId = null, string? details = null)
        {
            var log = new AuditLog
            {
                UserId = userId,
                Action = action,
                EntityName = entityName,
                EntityId = entityId,
                Details = details,
                CreatedAt = DateTime.UtcNow
            };

            _context.AuditLogs.Add(log);
            await _context.SaveChangesAsync();
        }
    }
}
