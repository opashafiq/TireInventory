using System.Drawing;

namespace TireInventory.Models.ReportDtos
{
    public class OURPTotalDto
    {
        public string Category {get;set;}
        public string Size { get; set; }
        public string Brand { get; set; }
        public string Series { get; set; }
        public string Bolt { get; set; }
        public string HoleS { get; set; }
        public string Zone { get; set; }
        public int Qty { get; set; }
        public decimal OURP { get; set; }
        public decimal Total { get; set; }
    }
}
