namespace TireInventory.Models.ReportDtos
{
    public class DailyReportDto
    {
        public string Date { get; set; }
        public string CategoryName { get; set; }
        public decimal TotalAmount { get; set; }
        public string? Paydate { get; set; }
    }
}
