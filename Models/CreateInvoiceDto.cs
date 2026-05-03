using System.Collections.Generic;

namespace TireInventory.Models
{
    // DTO used by CreateInvoice endpoint
    public class CreateInvoiceDto
    {
        public InvoiceMaster Invoice { get; set; } = new InvoiceMaster();
        public List<InvoiceDetails> InvoiceDetails { get; set; } = new List<InvoiceDetails>();
        public List<InvoicePayments> InvoicePayments { get; set; } = new List<InvoicePayments>();
    }
}