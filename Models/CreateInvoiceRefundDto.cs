using System.Collections.Generic;

namespace TireInventory.Models
{
    // DTO used by CreateRefund endpoint
    public class CreateInvoiceRefundDto
    {
        public InvoiceRefundMasterDto invoiceRefundMasterDto { get; set; } = new InvoiceRefundMasterDto();
        public List<InvoiceRefundDetailsDto> invoiceRefundDetailsDto { get; set; } = new List<InvoiceRefundDetailsDto>();
        public List<InvoiceRefundPaymentsDto> invoiceRefundPaymentsDto { get; set; } = new List<InvoiceRefundPaymentsDto>();
    }
}