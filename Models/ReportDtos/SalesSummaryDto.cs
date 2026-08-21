namespace TireInventory.Models.ReportDtos
{
    public class SalesSummaryDto
    {
        public string CategoryName { get; set; }
        public string Brand { get; set; }
        public string Size { get; set; }
        public int TotalQty { get; set; }
        public decimal AvgPrice { get; set; }
        public decimal tbim_OURP { get; set; }
        
    }
}
