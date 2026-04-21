using System;
using System.Collections.Generic;

namespace TireInventory.Models;

public partial class tbl_DistributorList
{
    public int Id { get; set; }

    public string tbdl_DistributorName { get; set; } = null!;

    public string? tbdl_DistributoAdd { get; set; }

    public string? UserName { get; set; }

    public DateTime? SetDate { get; set; }

    public virtual ICollection<ItemMaster> tbl_ItemMasters { get; set; } = new List<ItemMaster>();
}
