namespace TireInventory.Models
{
    public class LayawayRefundPaymentsDto
    {
        public long Id { get; set; }
        public long tbirp_Layaway_RefundId { get; set; }
        public long tbirp_RefundMethodId { get; set; }
        public decimal? tbirp_RefundAmt { get; set; }
        public DateTime? tbirp_Date { get; set; }

        // Resolved refund method name
        public string RefundMethodName { get; set; } = string.Empty;
    }
}