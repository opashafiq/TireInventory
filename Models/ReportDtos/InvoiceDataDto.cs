namespace TireInventory.Models.ReportDtos
{
    public class InvoiceDataDto
    {
        public string? tbip_Paymentname { get; set; }
        public long InvoiceId { get; set; }
        public string? Name { get; set; }
        public string InvoiceDate { get; set; }
        public decimal Total { get; set; }
        public string? CompanyName { get; set; }
        public string? TaxNumber { get; set; }
        public string? Phone { get; set; }
        public string? Address { get; set; }
        public string? Vehicle { get; set; }
        public string? Model { get; set; }
        public string? Year { get; set; }
        public string? Odometer { get; set; }
        public string? TreadDepth { get; set; }
        public string? License { get; set; }
        public string? Notes { get; set; }
    }
}
