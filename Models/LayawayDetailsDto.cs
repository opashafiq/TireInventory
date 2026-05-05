namespace TireInventory.Models
{
    public class LayawayDetailsDto
    {
        public long Id { get; set; }
        public long tbid_InvoiceId { get; set; }
        public long? tbid_ItemId { get; set; }
        public int? tbid_ItemCategory { get; set; }
        public string? tbid_DepartmentName { get; set; }
        public string? tbid_Size { get; set; }
        public string? tbid_Brand { get; set; }
        public string? tbid_Series { get; set; }
        public string? tbid_Bolt { get; set; }
        public string? tbid_HoleS { get; set; }
        public string? tbid_Zone { get; set; }
        public int? tbid_DistributorId { get; set; }
        public string? tbid_DistributorName { get; set; }
        public int tbid_Qty { get; set; }
        public bool tbid_Taxable { get; set; }
        public decimal tbid_UnitPrice { get; set; }
        public decimal tbid_LineTotal { get; set; }
        public decimal tbid_TaxAmt { get; set; }

        // resolved names
        public string ItemDepartmentName { get; set; } = string.Empty;
        public string ItemDistributorName { get; set; } = string.Empty;
        public string ItemLocationName { get; set; } = string.Empty;
        public string ItemDisplay { get; set; } = string.Empty;
    }
}