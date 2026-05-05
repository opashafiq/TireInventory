namespace TireInventory.Models
{
    public class LayawayRefundMasterDto
    {
        public long Id { get; set; }
        public DateTime tbirm_LayawayRefundDate { get; set; }
        public string tbirm_RefundType { get; set; } = string.Empty;
        public decimal tbirm_SubTotal { get; set; }
        public decimal tbirm_SaleTax { get; set; }
        public decimal tbirm_Labour { get; set; }
        public decimal tbirm_DisPer { get; set; }
        public decimal tbirm_DisAmt { get; set; }
        public decimal tbirm_Total { get; set; }
        public decimal tbirm_RefundAmt { get; set; }
        public string? tbirm_Note { get; set; }
        public string UserName { get; set; } = string.Empty;
        public DateTime SetDate { get; set; }

        // Resolved original layaway info (if available)
        public string OriginalLayawayName { get; set; } = string.Empty;
        public DateTime? OriginalLayawayDate { get; set; }
    }
}