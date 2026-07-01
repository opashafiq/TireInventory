using System.Collections.Generic;

namespace TireInventory.Models
{
    // DTO used by CreateInvoice endpoint
    public class CreateInvoiceDto
    {
        public InvoiceMasterDto invoiceMasterDto { get; set; } = new InvoiceMasterDto();
        public List<InvoiceDetailsDto> invoiceDetailsDto { get; set; } = new List<InvoiceDetailsDto>();
        public List<InvoicePaymentsDto> invoicePaymentsDto { get; set; } = new List<InvoicePaymentsDto>();
    }
}