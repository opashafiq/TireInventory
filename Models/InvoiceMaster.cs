using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace TireInventory.Models;

[Table("tbl_Invoice_Master")]
public partial class InvoiceMaster
{
    public long Id { get; set; }
    public long? tbim_InvoiceIdRad { get; set; }
    public string tbim_Phone { get; set; } = null!;
    public DateTime tbim_InvDate { get; set; }
    public string tbim_Name { get; set; } = null!;    
    public long? tbim_TaxId { get; set; }
    public string? tbim_VehicleMake { get; set; }
    public string? tbim_Model { get; set; }
    public string? tbim_Year { get; set; }
    public string? tbim_Odometer { get; set; }
    public string? tbim_TreadDepth { get; set; }
    public string? tbim_License { get; set; }
    public decimal tbim_SubTotal { get; set; }
    public decimal tbim_SaleTax { get; set; }
    public decimal tbim_Labour { get; set; }
    public decimal tbim_DisPer { get; set; }
    public decimal tbim_DisAmt { get; set; }
    public decimal tbim_Total { get; set; }
    public decimal tbim_PaidAmt { get; set; }
    public decimal tbim_AdjAmt { get; set; }
    public decimal tbim_AdjTotal { get; set; }
    public string? tbim_PayInfo { get; set; }
    public string? tbim_Note { get; set; }
    public string? tbim_Delinfo { get; set; }    
    public string? tbim_CompanyName { get; set; }
    public string? tbim_CompanyAddress { get; set; }
    public bool tbim_Item_Delete_after_Invoice_Create { get; set; }
    public long? tbim_LaywayNo { get; set; }
    public DateTime? tbim_LaywayDate { get; set; }
    public string UserName { get; set; } = null!;
    public DateTime SetDate { get; set; }
    public bool tbim_Left_Front { get; set; }
    public bool tbim_Right_Front { get; set; }
    public bool tbim_Left_Rear { get; set; }
    public bool tbim_Right_Rear { get; set; }
    public string? tbim_EmailAddress { get; set; }
    public string? tbim_IDNo { get; set; }
    public string? tbim_RefundType { get; set; }
    [Column("tbim_LocationId")]
    public long? tbim_LocationDetailsId { get; set; }

    // Foreign keys
    public virtual LocationDetails? tbim_LocationDetails { get; set; }
    public virtual TaxId? tbim_Tax { get; set; }
    public virtual ICollection<InvoiceDetails> InvoiceDetails { get; set; } = new List<InvoiceDetails>();
    public virtual ICollection<InvoicePayments> InvoicePayments { get; set; } = new List<InvoicePayments>();
    public virtual ICollection<InvoiceRefundMaster> InvoiceRefundMasters { get; set; } = new List<InvoiceRefundMaster>();
}
