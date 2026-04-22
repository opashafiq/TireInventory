using System;
using System.Collections.Generic;

namespace TireInventory.Models;

public partial class InvoiceRefundMaster
{
    public long Id { get; set; }

    public long? tbirm_InvoiceRefundIdRad { get; set; }

    public DateTime tbirm_InvRefundDate { get; set; }

    /// <summary>
    /// Value must in F: Full or P:Partial
    /// </summary>
    public string tbirm_RefundType { get; set; } = null!;

    public long tbirm_InvoiceId { get; set; }

    public decimal tbirm_SubTotal { get; set; }

    public decimal tbirm_SaleTax { get; set; }

    public decimal tbirm_Labour { get; set; }

    public decimal tbirm_DisPer { get; set; }

    public decimal tbirm_DisAmt { get; set; }

    public decimal tbirm_Total { get; set; }

    public decimal tbirm_RefundAmt { get; set; }

    public decimal tbirm_AdjAmt { get; set; }

    public string? tbirm_Note { get; set; }

    public string? tbirm_Delinfo { get; set; }

    public bool tbirm_Item_Delete_after_Invoice_Refund_Create { get; set; }

    public string UserName { get; set; } = null!;

    public DateTime SetDate { get; set; }

    // Foreign key
    public virtual InvoiceMaster tbirm_Invoice { get; set; }

    public virtual ICollection<InvoiceRefundDetails> tbl_Invoice_Refund_Details { get; set; } = new List<InvoiceRefundDetails>();

    public virtual ICollection<InvoiceRefundPayments> tbl_Invoice_Refund_Payments { get; set; } = new List<InvoiceRefundPayments>();
}
