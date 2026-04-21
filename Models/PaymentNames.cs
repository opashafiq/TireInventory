using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace TireInventory.Models;
[Table("tbl_PaymentName")]
public partial class PaymentNames
{
    public long Id { get; set; }

    public string tbpn_PaymentName { get; set; } = null!;

    public bool tbpn_IsActive { get; set; }

    public string UserName { get; set; } = null!;

    public DateTime SetDate { get; set; }
}
