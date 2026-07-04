namespace TireInventory.Models
{
    // Projection DTO for InvoicePayments including resolved payment name
    public class InvoicePaymentsDto
    {
        public long Id { get; set; }
        public long tbip_InvoiceId { get; set; }
        public long tbip_PaymentId { get; set; }
        public decimal? tbip_PayAmt { get; set; }
        public DateTime? tbip_Date { get; set; }
        public string? tbip_PaymentType { get; set; }
        public long? tbip_LayawayId { get; set; }
        public string? tdip_fromlayaway { get; set; }
        public DateTime? tbip_LayawayDate { get; set; }

        // Resolved name from PaymentNames
        public string PaymentName { get; set; } = string.Empty;
    }
}