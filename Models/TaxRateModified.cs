using System;
using System.Collections.Generic;

namespace TireInventory.Models;

public partial class TaxRateModified
{
    public long Id { get; set; }

    public string tbtm_Note { get; set; } = null!;

    public string UserName { get; set; } = null!;

    public DateTime SetDate { get; set; }
}
