using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace TireInventory.Models;
[Table("tbl_Invoice_Refund_Details")]
public partial class InvoiceRefundDetails
{
    public long Id { get; set; }
    public long tbird_InvoiceRefundId { get; set; }
    public long? tbird_ItemId { get; set; }
    public int? tbird_ItemCategory { get; set; }
    public string? tbird_DepartmentName { get; set; }
    public string? tbird_Size { get; set; }
    public string? tbird_Brand { get; set; }
    public string? tbird_Series { get; set; }
    public string? tbird_Bolt { get; set; }
    public string? tbird_HoleS { get; set; }
    public string? tbird_Zone { get; set; }
    public int? tbird_DistributorId { get; set; }
    public string? tbird_DistributorName { get; set; }
    public int tbird_Qty { get; set; }
    public bool tbird_Taxable { get; set; }
    public decimal tbird_UnitPrice { get; set; }
    public decimal tbird_LineTotal { get; set; }
    public decimal tbird_TaxAmt { get; set; }

    public virtual InvoiceRefundMaster tbird_InvoiceRefund { get; set; } = null!;
    public virtual ItemMaster? tbird_Item { get; set; }

}
