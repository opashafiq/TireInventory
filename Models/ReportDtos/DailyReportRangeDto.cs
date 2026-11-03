namespace TireInventory.Models.ReportDtos
{
    public class DailyReportRangeDto
    {
        public string StartDate { get; set; }
        public string CategoryName { get; set; }
        public decimal TotalAmount { get; set; }
        public string? Paydate { get; set; }
        public string EndDate { get; set; }
    }
}
