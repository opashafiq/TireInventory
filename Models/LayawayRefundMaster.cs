using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace TireInventory.Models;

public partial class LayawayRefundMaster
{
    public long Id { get; set; }

    public long? tbirm_LayawayRefundIdRad { get; set; }

    public DateTime tbirm_LayawayRefundDate { get; set; }

    public string tbirm_RefundType { get; set; } = null!;

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

    public bool tbirm_Item_Delete_after_Layaway_Refund_Create { get; set; }

    public string UserName { get; set; } = null!;

    public DateTime SetDate { get; set; }

    public long Layaway_tbim_InvoiceId { get; set; }

    public long? Layaway_tbim_InvoiceIdRad { get; set; }

    public string? Layaway_tbim_Phone { get; set; }

    public DateTime Layaway_tbim_InvDate { get; set; }

    public string? Layaway_tbim_Name { get; set; }

    public long? Layaway_tbim_TaxId { get; set; }

    public string? Layaway_tbim_VehicleMake { get; set; }

    public string? Layaway_tbim_Model { get; set; }

    public string? Layaway_tbim_Year { get; set; }

    public string? Layaway_tbim_Odometer { get; set; }

    public string? Layaway_tbim_TreadDepth { get; set; }

    public string? Layaway_tbim_License { get; set; }

    public decimal Layaway_tbim_SubTotal { get; set; }

    public decimal Layaway_tbim_SaleTax { get; set; }

    public decimal Layaway_tbim_Labour { get; set; }

    public decimal Layaway_tbim_DisPer { get; set; }

    public decimal Layaway_tbim_DisAmt { get; set; }

    public decimal Layaway_tbim_Total { get; set; }

    public decimal Layaway_tbim_PaidAmt { get; set; }

    public decimal Layaway_tbim_AdjAmt { get; set; }

    public decimal Layaway_tbim_AdjTotal { get; set; }

    public string? Layaway_tbim_PayInfo { get; set; }

    public string? Layaway_tbim_Note { get; set; }

    public string? Layaway_tbim_Delinfo { get; set; }

    public string? Layaway_tbim_CompanyName { get; set; }

    public string? Layaway_tbim_CompanyAddress { get; set; }

    public bool Layaway_tbim_Item_Delete_after_Layaway_Create { get; set; }

    public bool Layaway_tbim_Left_Front { get; set; }

    public bool Layaway_tbim_Right_Front { get; set; }

    public bool Layaway_tbim_Left_Rear { get; set; }

    public bool Layaway_tbim_Right_Rear { get; set; }

    public string? Layaway_tbim_EmailAddress { get; set; }

    public string? Layaway_tbim_IDNo { get; set; }
    // Foreign key relationships
    public virtual TaxId? Layaway_tbim_Tax { get; set; }
    public virtual ICollection<LayawayRefundDetails> tbl_Layaway_Refund_Details { get; set; } = new List<LayawayRefundDetails>();

    public virtual ICollection<LayawayRefundPayments> tbl_Layaway_Refund_Payments { get; set; } = new List<LayawayRefundPayments>();
}
