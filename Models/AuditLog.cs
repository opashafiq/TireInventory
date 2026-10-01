using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TireInventory.Models
{
    public enum AuditActionType
    {
        Create = 1,
        Update = 2,
        Delete = 3
    }

    public class AuditLog
    {
        [Key]
        public long Id { get; set; }

        [Required]
        [MaxLength(256)]
        public string TableName { get; set; }

        // JSON containing the primary key(s) of the affected row(s). Example: {"Id":123}
        public string KeyValues { get; set; }

        // JSON payload of values before the change (for Update/Delete)
        public string OldValues { get; set; }

        // JSON payload of values after the change (for Create/Update)
        public string NewValues { get; set; }

        // Comma separated list or JSON array of changed column names
        public string ChangedColumns { get; set; }

        [Required]
        public AuditActionType Action { get; set; }

        // Who made the change. Keep as string to allow GUIDs or usernames.
        [MaxLength(256)]
        public string UserId { get; set; }

        [MaxLength(256)]
        public string UserName { get; set; }

        // Timestamp stored in UTC
        public DateTimeOffset CreatedAt { get; set; }

        [MaxLength(100)]
        public string IpAddress { get; set; }

        public string UserAgent { get; set; }

        [MaxLength(100)]
        public string CorrelationId { get; set; }

        public string RequestPath { get; set; }

        // Optional tenant identifier for multi-tenant systems
        [MaxLength(100)]
        public string TenantId { get; set; }

        public AuditLog()
        {
            CreatedAt = DateTimeOffset.UtcNow;
        }
    }
}
