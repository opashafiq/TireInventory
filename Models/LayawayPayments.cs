using System;
using System.Collections.Generic;

namespace TireInventory.Models;

public partial class LayawayPayments
{
    public long Id { get; set; }

    public long tbip_InvoiceId { get; set; }

    public long tbip_PaymentId { get; set; }

    public decimal? tbip_PayAmt { get; set; }

    public DateTime? tbip_Date { get; set; }

    public string? tbip_PaymentType { get; set; }

    public virtual LayawayMaster tbip_Invoice { get; set; } = null!;
    public virtual PaymentNames tbip_Payment { get; set; } = null!;
}
