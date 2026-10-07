using Microsoft.AspNetCore.Http;
using System.Text.Json;
using System.Threading.Tasks;
using TireInventory.Data;
using TireInventory.Models;

namespace TireInventory.Services
{
    public class AuditService : IAuditService
    {
        private readonly ApplicationDBContext _context;

        public AuditService(ApplicationDBContext context)
        {
            _context = context;
        }

        public async Task LogAsync(HttpContext httpContext,
                                   string @event,
                                   AuditEventType eventType,
                                   string entityName,
                                   string? entityId = null,
                                   object? changes = null,
                                   string? description = null,
                                   object? metadata = null,
                                   string? performedById = null,
                                   string? performedByName = null,
                                   bool isSuccess = true)
        {
            var audit = new AuditTrail
            {
                Event = @event,
                EventType = eventType,
                EntityName = entityName,
                EntityId = entityId,
                Changes = changes == null ? null : JsonSerializer.Serialize(changes),
                Description = description,
                IsSuccess = isSuccess,
                Metadata = metadata == null ? null : JsonSerializer.Serialize(metadata),
                PerformedById = performedById,
                PerformedByName = performedByName,
                IpAddress = httpContext.Connection.RemoteIpAddress?.ToString(),
                UserAgent = httpContext.Request.Headers["User-Agent"].ToString(),
                CorrelationId = httpContext.Request.Headers["X-Correlation-ID"].ToString()
            };

            _context.AuditTrails.Add(audit);
            await _context.SaveChangesAsync();
        }
    }
}
