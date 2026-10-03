using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TireInventory.Data;
using TireInventory.Models;

namespace TireInventory.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuditTrailController : ControllerBase
    {
        private readonly ApplicationDBContext _context;

        public AuditTrailController(ApplicationDBContext context)
        {
            _context = context;
        }

        // GET: api/AuditTrail
        // Supports filtering by: Event, EventType, EntityName, PerformedById, PerformedByName, IpAddress
        [HttpGet]
        public async Task<ActionResult<PagedAuditTrailResponseDto>> GetAuditTrails(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery(Name = "Event")] string eventName = null,
            [FromQuery(Name = "EventType")] int? eventType = null,
            [FromQuery] string entityName = null,
            [FromQuery] string performedById = null,
            [FromQuery] string performedByName = null,
            [FromQuery] string ipAddress = null,
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null)
        {
            if (pageSize > 100) pageSize = 100;
            if (pageNumber < 1) pageNumber = 1;

            var query = _context.AuditTrails.AsNoTracking();

            if (!string.IsNullOrWhiteSpace(eventName))
            {
                var v = eventName.Trim();
                query = query.Where(a => a.Event.Contains(v));
            }

            if (eventType.HasValue)
            {
                var et = (AuditEventType)eventType.Value;
                query = query.Where(a => a.EventType == et);
            }

            if (!string.IsNullOrWhiteSpace(entityName))
            {
                var v = entityName.Trim();
                query = query.Where(a => a.EntityName.Contains(v));
            }

            if (!string.IsNullOrWhiteSpace(performedById))
            {
                var v = performedById.Trim();
                query = query.Where(a => a.PerformedById.Contains(v));
            }

            if (!string.IsNullOrWhiteSpace(performedByName))
            {
                var v = performedByName.Trim();
                query = query.Where(a => a.PerformedByName.Contains(v));
            }

            if (!string.IsNullOrWhiteSpace(ipAddress))
            {
                var v = ipAddress.Trim();
                query = query.Where(a => a.IpAddress.Contains(v));
            }

            if (startDate.HasValue)
            {
                query = query.Where(a => a.PerformedAt >= startDate.Value.Date);
            }

            if (endDate.HasValue)
            {
                var inclusiveEnd = endDate.Value.Date.AddDays(1).AddTicks(-1);
                query = query.Where(a => a.PerformedAt <= inclusiveEnd);
            }

            int totalRecords = await query.CountAsync();
            int totalPages = (int)Math.Ceiling((double)totalRecords / pageSize);

            var items = await query
                .OrderByDescending(a => a.PerformedAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var dtoList = items.Select(a => new AuditTrailDto
            {
                Id = a.Id,
                Event = a.Event,
                EventType = (int)a.EventType,
                EventTypeName = a.EventType.ToString(),
                EntityName = a.EntityName,
                EntityId = a.EntityId,
                Changes = a.Changes,
                Description = a.Description,
                PerformedById = a.PerformedById,
                PerformedByName = a.PerformedByName,
                PerformedAt = a.PerformedAt,
                IpAddress = a.IpAddress,
                UserAgent = a.UserAgent,
                CorrelationId = a.CorrelationId,
                IsSuccess = a.IsSuccess,
                Metadata = a.Metadata
            }).ToList();

            var response = new PagedAuditTrailResponseDto
            {
                Items = dtoList,
                TotalCount = totalRecords,
                PageNumber = pageNumber,
                TotalPages = totalPages
            };

            return Ok(response);
        }

        // GET: api/AuditTrail/EventTypes
        // Returns AuditEventType enum as key-value pairs for frontend combo boxes
        [HttpGet("EventTypes")]
        public ActionResult<IEnumerable<object>> GetEventTypes()
        {
            var list = Enum.GetValues(typeof(AuditEventType))
                .Cast<AuditEventType>()
                .Select(e => new { Key = (int)e, Value = e.ToString() })
                .ToList();

            return Ok(list);
        }
    }
}