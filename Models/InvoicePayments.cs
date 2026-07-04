using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace TireInventory.Models;

[Table("tbl_Invoice_Payment")]
public partial class InvoicePayments
{
    public long Id { get; set; }
    public long tbip_InvoiceId { get; set; }
    public long tbip_PaymentId { get; set; }
    public decimal? tbip_PayAmt { get; set; }
    public DateTime? tbip_Date { get; set; }
    public string? tbip_PaymentType { get; set; }
    public long? tbip_LayawayId { get; set; }
    public string? tdip_fromlayaway { get; set; }
    public DateTime? tbip_LayawayDate { get; set; }

    public virtual InvoiceMaster tbip_Invoice { get; set; } = null!;
    // Tell EF Core that "tbip_PaymentId" is the foreign key for this navigation property
    [ForeignKey(nameof(tbip_PaymentId))]
    public virtual PaymentNames? tbip_Payment { get; set; } 
}
