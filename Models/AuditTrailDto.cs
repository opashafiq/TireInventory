using System;

namespace TireInventory.Models
{
    public class AuditTrailDto
    {
        public long Id { get; set; }
        public string Event { get; set; }
        public int EventType { get; set; }
        public string EventTypeName { get; set; }
        public string EntityName { get; set; }
        public string EntityId { get; set; }
        public string Changes { get; set; }
        public string Description { get; set; }
        public string PerformedById { get; set; }
        public string PerformedByName { get; set; }
        public DateTimeOffset PerformedAt { get; set; }
        public string IpAddress { get; set; }
        public string UserAgent { get; set; }
        public string CorrelationId { get; set; }
        public bool IsSuccess { get; set; }
        public string Metadata { get; set; }
    }
}