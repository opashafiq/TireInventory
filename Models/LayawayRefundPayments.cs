using System;
using System.Collections.Generic;

namespace TireInventory.Models;

public partial class LayawayRefundPayments
{
    public long Id { get; set; }

    public long tbirp_Layaway_RefundId { get; set; }

    public long tbirp_RefundMethodId { get; set; }

    public decimal? tbirp_RefundAmt { get; set; }

    public DateTime? tbirp_Date { get; set; }

    public virtual LayawayRefundMaster tbirp_Layaway_Refund { get; set; } = null!;
}
