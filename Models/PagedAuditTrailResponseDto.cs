using System.Collections.Generic;

namespace TireInventory.Models
{
    public class PagedAuditTrailResponseDto
    {
        public List<AuditTrailDto> Items { get; set; } = new List<AuditTrailDto>();
        public int TotalCount { get; set; }
        public int PageNumber { get; set; }
        public int TotalPages { get; set; }
    }
}