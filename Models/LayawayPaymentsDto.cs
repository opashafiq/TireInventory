namespace TireInventory.Models
{
    public class LayawayPaymentsDto
    {
        public long Id { get; set; }
        public long tbip_InvoiceId { get; set; }
        public long tbip_PaymentId { get; set; }
        public decimal? tbip_PayAmt { get; set; }
        public DateTime? tbip_Date { get; set; }
        public string? tbip_PaymentType { get; set; }

        // resolved name
        public string PaymentName { get; set; } = string.Empty;
    }
}