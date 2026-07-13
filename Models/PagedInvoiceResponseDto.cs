namespace TireInventory.Models
{
    public class PagedInvoiceResponseDto
    {
        public List<CreateInvoiceDto> Items { get; set; } = new List<CreateInvoiceDto>();
        public int TotalCount { get; set; }
        public int PageNumber { get; set; }
        public int TotalPages { get; set; }
    }
}
