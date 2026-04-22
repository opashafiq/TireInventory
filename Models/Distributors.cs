using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TireInventory.Models
{
    [Table("tbl_DistributorList")]
    public class Distributors
    {
        [Key]
        public int Id { get; set; }
        [Required]
        [Column("tbdl_DistributorName")]
        public string Name { get; set; }
        [Column("tbdl_DistributoAdd")]
        public string? Address { get; set; }
        public string UserName { get; set; }
        public DateTime? SetDate { get; set; }

        public virtual ICollection<ItemMaster> tbl_ItemMasters { get; set; } = new List<ItemMaster>();

    }
}
