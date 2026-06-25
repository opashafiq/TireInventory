using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace TireInventory.Models;
[Table("tbl_BO_LocationDetails")]
public partial class LocationDetails
{
    public long Id { get; set; }

    public string tbld_LocationName { get; set; } = null!;

    [Column("tbld_BusinessId")]
    public long CompanyInfoId { get; set; }

    public string? tbld_Address1 { get; set; }

    public string? tbld_Address2 { get; set; }

    public string? tbld_City { get; set; }

    public string? tbld_State { get; set; }

    public string? tbld_ZipCode { get; set; }

    public string? tbld_Phone { get; set; }

    public string? tbld_Fax { get; set; }

    public string? tbld_Email { get; set; }

    public string UserName { get; set; } = null!;

    public DateTime SetDate { get; set; }

    public virtual CompanyInfo CompanyInfo { get; set; } = null!;

    public virtual ICollection<ItemMaster> tbl_ItemMasters { get; set; } = new List<ItemMaster>();
    public virtual ICollection<DailyExpense> DailyExpense { get; set; } = new List<DailyExpense>();
}
