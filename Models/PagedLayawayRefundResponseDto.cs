namespace TireInventory.Models
{
    public class PagedLayawayRefundResponseDto
    {
        public List<CreateLayawayRefundDto> Items { get; set; } = new List<CreateLayawayRefundDto>();
        public int TotalCount { get; set; }
        public int PageNumber { get; set; }
        public int TotalPages { get; set; }
    }
}
