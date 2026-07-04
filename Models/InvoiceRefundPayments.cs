using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace TireInventory.Models;

[Table("tbl_Invoice_Refund_Payment")]
public partial class InvoiceRefundPayments
{
    public long Id { get; set; }
    public long tbirp_InvoiceRefundId { get; set; }
    public long tbirp_RefundMethodId { get; set; }
    public decimal? tbirp_RefundAmt { get; set; }
    public DateTime? tbirp_Date { get; set; }

    public virtual InvoiceRefundMaster tbirp_InvoiceRefund { get; set; } = null!;
    public virtual RefundMethodNames tbirp_RefundMethod { get; set; } = null!;
}
