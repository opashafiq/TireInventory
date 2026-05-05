namespace TireInventory.Models
{
    // DTO for InvoiceRefundPayments with resolved refund method name
    public class InvoiceRefundPaymentsDto
    {
        public long Id { get; set; }
        public long tbirp_InvoiceRefundId { get; set; }
        public long tbirp_RefundMethodId { get; set; }
        public decimal? tbirp_RefundAmt { get; set; }
        public DateTime? tbirp_Date { get; set; }

        // Resolved refund method name
        public string RefundMethodName { get; set; } = string.Empty;
    }
}