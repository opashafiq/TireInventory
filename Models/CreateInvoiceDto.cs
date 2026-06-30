using System.Collections.Generic;

namespace TireInventory.Models
{
    // DTO used by CreateInvoice endpoint
    public class CreateInvoiceDto
    {
        public InvoiceMasterDto Invoice { get; set; } = new InvoiceMasterDto();
        public List<InvoiceDetailsDto> InvoiceDetails { get; set; } = new List<InvoiceDetailsDto>();
        public List<InvoicePaymentsDto> InvoicePayments { get; set; } = new List<InvoicePaymentsDto>();
    }
}