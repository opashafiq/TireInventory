using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace TireInventory.Models;
[Table("tbl_TaxRateModified")]
public partial class TaxRateModified
{
    public long Id { get; set; }
    public decimal tbtm_TaxRate { get; set; }

    public string tbtm_Note { get; set; } = null!;

    public string UserName { get; set; } = null!;

    public DateTime SetDate { get; set; }
}
