namespace TireInventory.Models
{
    public class PagedLayawayResponseDto
    {
        public List<CreateLayawayDto> Items { get; set; } = new List<CreateLayawayDto>();
        public int TotalCount { get; set; }
        public int PageNumber { get; set; }
        public int TotalPages { get; set; }
    }
}
