using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace TireInventory.Models;

public partial class InvoicePayments
{
    public long Id { get; set; }

    public long tbip_InvoiceId { get; set; }

    public long tbip_PaymentId { get; set; }

    public decimal? tbip_PayAmt { get; set; }

    public DateTime? tbip_Date { get; set; }

    /// <summary>
    /// P: For Payment with Invoice, D: Pending Pyment
    /// </summary>
    [Column("tbip_PaymentType")]
    public string? tbip_PaymentTypeId { get; set; }

    public long? tbip_LayawayId { get; set; }

    public string? tdip_fromlayaway { get; set; }

    public DateTime? tbip_LayawayDate { get; set; }

    public virtual InvoiceMaster tbip_Invoice { get; set; } = null!;
    public virtual PaymentNames? tbip_PaymentType { get; set; } 
}
