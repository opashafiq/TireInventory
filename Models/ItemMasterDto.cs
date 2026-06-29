namespace TireInventory.Models
{
    // Projection DTO for ItemMaster with resolved FK names
    public class ItemMasterDto
    {
        public long Id { get; set; }

        public long tbim_ItemCategoryId { get; set; }
        public string? tbim_Size { get; set; }
        public string? tbim_Brand { get; set; }
        public string? tbim_Series { get; set; }
        public string? tbim_Bolt { get; set; }
        public string? tbim_HoleS { get; set; }
        public string? tbim_Zone { get; set; }
        public int tbim_Qty { get; set; }
        public int? tbim_QtyOp { get; set; }
        public decimal tbim_Code { get; set; }
        public decimal tbim_CodeTOT { get; set; }
        public int? tbim_DistributorId { get; set; }
        public decimal? tbim_OURP { get; set; }
        public long tbim_LocationId { get; set; }
        public DateTime? tbim_ThrashDate { get; set; }
        public string UserName { get; set; } = null!;
        public DateTime SetDate { get; set; }

        // Resolved names for foreign keys
        public string DepartmentName { get; set; } = string.Empty;
        public string DistributorName { get; set; } = string.Empty;
        public string LocationName { get; set; } = string.Empty;
    }
}