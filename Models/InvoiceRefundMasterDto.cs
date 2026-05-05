namespace TireInventory.Models
{
    // DTO for InvoiceRefundMaster with resolved original invoice info
    public class InvoiceRefundMasterDto
    {
        public long Id { get; set; }
        public long tbirm_InvoiceId { get; set; }
        public DateTime tbirm_InvRefundDate { get; set; }
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

        // Resolved original invoice information
        public string OriginalInvoiceName { get; set; } = string.Empty;
        public DateTime? OriginalInvoiceDate { get; set; }
    }
}