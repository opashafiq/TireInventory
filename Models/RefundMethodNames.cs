using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace TireInventory.Models;
[Table("tbl_RefundMethodName")]
public partial class RefundMethodNames
{
    public long Id { get; set; }

    public string tbrmn_RefundMethodName { get; set; } = null!;

    public bool tbrmn_IsActive { get; set; }

    public string? UserName { get; set; }

    public DateTime? SetDate { get; set; }
}
