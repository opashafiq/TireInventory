using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace TireInventory.Models;
[Table("tbl_TaxId")]
public partial class TaxId
{
    public long Id { get; set; }

    public string tbti_ComName { get; set; } = null!;

    public string tbti_TaxNumber { get; set; } = null!;

    public string tbti_Address { get; set; } = null!;

    public string? tbti_Phone { get; set; }

    public string UserName { get; set; } = null!;

    public DateTime SetDate { get; set; }
}
