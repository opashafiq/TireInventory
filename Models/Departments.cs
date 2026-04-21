using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace TireInventory.Models;

[Table("tbl_BO_ItemDepartment")]
public partial class Department
{
    public long Id { get; set; }

    public string Tbid_DepartmentName { get; set; } = null!;

    public bool Tbid_IsActive { get; set; }

    public string UserName { get; set; } = null!;

    public DateTime SetDate { get; set; }

    //public virtual ICollection<TblItemMaster> TblItemMasters { get; set; } = new List<TblItemMaster>();
}
