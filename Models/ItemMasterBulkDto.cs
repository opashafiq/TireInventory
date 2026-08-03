using System.ComponentModel.DataAnnotations.Schema;

namespace TireInventory.Models
{
    public class ItemMasterBulkDto
    {
        public long Id { get; set; }

        [Column("tbim_ItemCategory")]
        public string tbim_ItemCategoryName { get; set; }

        public string tbim_Size { get; set; } = null!;

        public string tbim_Brand { get; set; } = null!;

        public string? tbim_Series { get; set; }

        public string? tbim_Bolt { get; set; }

        public string? tbim_HoleS { get; set; }

        public string? tbim_Zone { get; set; }

        public int tbim_Qty { get; set; }

        public int? tbim_QtyOp { get; set; }

        public decimal tbim_Code { get; set; }

        public decimal tbim_CodeTOT { get; set; }

        public string? tbim_DistributorName { get; set; }

        public decimal? tbim_OURP { get; set; }

        public DateTime? tbim_ThrashDate { get; set; }

        public string UserName { get; set; } = null!;

        /// <summary>
        /// getdate()
        /// </summary>
        public DateTime SetDate { get; set; }

        public long tbim_LocationId { get; set; }
    }
}
