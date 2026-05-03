using System.Collections.Generic;

namespace TireInventory.Models
{
    // DTO used by CreateRefund endpoint
    public class CreateInvoiceRefundDto
    {
        public InvoiceRefundMaster Refund { get; set; } = new InvoiceRefundMaster();
        public List<InvoiceRefundDetails> RefundDetails { get; set; } = new List<InvoiceRefundDetails>();
        public List<InvoiceRefundPayments> RefundPayments { get; set; } = new List<InvoiceRefundPayments>();
    }
}