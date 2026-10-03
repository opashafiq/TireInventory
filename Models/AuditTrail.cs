using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TireInventory.Models
{
    /// <summary>
    /// High-level event types for audit trails.
    /// </summary>
    public enum AuditEventType
    {
        Create = 1,
        Read = 2,
        Update = 3,
        Delete = 4,
        Login = 10,
        Logout = 11,
        PermissionChange = 20,
        Other = 99
    }

    /// <summary>
    /// Represents an audit trail entry recording who performed an action, when, and what changed.
    /// Designed for storing structured JSON for changes and metadata.
    /// </summary>
    public class AuditTrail
    {
        [Key]
        public long Id { get; set; }

        // A short, human-friendly name for the event (eg: "Update Department")
        [Required]
        [MaxLength(200)]
        public string? Event { get; set; }

        [Required]
        public AuditEventType EventType { get; set; }

        // The affected entity/table name (eg: "Departments")
        [MaxLength(200)]
        public string? EntityName { get; set; }

        // The primary key or identifier for the affected entity. Keep as string to allow composite keys or JSON.
        [MaxLength(200)]
        public string? EntityId { get; set; }

        // JSON payload describing the changed properties. Example: [{ "Property": "Name", "Old": "A", "New": "B" }]
        public string? Changes { get; set; }

        // Optional free-form description or message about the event
        public string? Description { get; set; }

        // Who performed the action. Keep as string to support usernames or GUIDs from different identity providers.
        [MaxLength(200)]
        public string? PerformedById { get; set; }

        [MaxLength(200)]
        public string? PerformedByName { get; set; }

        // UTC timestamp when the event occurred
        public DateTimeOffset PerformedAt { get; set; }

        // Request metadata
        [MaxLength(100)]
        public string? IpAddress { get; set; }

        public string? UserAgent { get; set; }

        [MaxLength(100)]
        public string? CorrelationId { get; set; }

        // Indicates success/failure of the operation (useful for login attempts etc.)
        public bool IsSuccess { get; set; }

        // Additional structured metadata (JSON) for extensibility
        public string? Metadata { get; set; }

        public AuditTrail()
        {
            PerformedAt = DateTimeOffset.UtcNow;
            IsSuccess = true;
        }
    }
}
