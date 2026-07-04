using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace TireInventory.Models;
[Table("tbl_Layaway_Payment")]
public partial class LayawayPayments
{
    public long Id { get; set; }
    public long tbip_InvoiceId { get; set; }
    public long tbip_PaymentId { get; set; }
    public decimal? tbip_PayAmt { get; set; }
    public DateTime? tbip_Date { get; set; }
    public string? tbip_PaymentType { get; set; }
    public virtual LayawayMaster tbip_Invoice { get; set; } = null!;
    // Tell EF Core that "tbip_PaymentId" is the foreign key for this navigation property
    [ForeignKey(nameof(tbip_PaymentId))]
    public virtual PaymentNames? tbip_Payment { get; set; } = null!;
}
