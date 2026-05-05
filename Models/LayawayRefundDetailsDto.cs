namespace TireInventory.Models
{
    public class LayawayRefundDetailsDto
    {
        public long Id { get; set; }
        public long tbird_Layaway_RefundId { get; set; }
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
        public int tbird_Layaway_Qty { get; set; }
        public decimal? tbird_Layaway_Qty_LineTotal { get; set; }
        public decimal? tbird_Layaway_Qty_TaxAmt { get; set; }
        public bool tbird_Taxable { get; set; }
        public decimal tbird_UnitPrice { get; set; }
        public decimal tbird_LineTotal { get; set; }
        public decimal? tbird_TaxRate { get; set; }
        public decimal tbird_TaxAmt { get; set; }

        // Resolved names
        public string ItemDepartmentName { get; set; } = string.Empty;
        public string ItemDistributorName { get; set; } = string.Empty;
        public string ItemLocationName { get; set; } = string.Empty;
        public string ItemDisplay { get; set; } = string.Empty;
    }
}