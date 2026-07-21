namespace TireInventory.Models
{
    public class PagedInvoiceRefundResponseDto
    {
        public List<CreateInvoiceRefundDto> Items { get; set; } = new List<CreateInvoiceRefundDto>();
        public int TotalCount { get; set; }
        public int PageNumber { get; set; }
        public int TotalPages { get; set; }
    }
}
