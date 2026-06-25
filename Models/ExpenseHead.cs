using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace TireInventory.Models;
[Table("tbl_ExpenseHead")]
public partial class ExpenseHead
{
    public long Id { get; set; }

    public string tbeh_HeadName { get; set; } = null!;

    public string? UserName { get; set; }

    public DateTime? SetDate { get; set; }

    //public virtual ICollection<DailyExpense> DailyExpense { get; set; } = new List<DailyExpense>();
}
