using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;
using TireInventory.Models;

namespace TireInventory.Services
{
    public interface IAuditService
    {
        Task LogAsync(HttpContext httpContext,
                      string @event,
                      AuditEventType eventType,
                      string entityName,
                      string? entityId = null,
                      object? changes = null,
                      string? description = null,
                      object? metadata = null,
                      string? performedById = null,
                      string? performedByName = null,
                      bool isSuccess = true);
    }
}
