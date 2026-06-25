using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace TireInventory.Models;
[Table("tblDailyExpense")]
public partial class DailyExpense
{
    public long Id { get; set; }

    public long ExpenseHeadId { get; set; }

    public DateTime ExpenseDate { get; set; }

    public decimal? Amount { get; set; }

    public string? CheckNo { get; set; }

    public string? PayType { get; set; }

    public string? UserName { get; set; }

    public DateTime? SetDate { get; set; }
    [Column("LocationId")]
    public long? LocationDetailsId { get; set; }
    [NotMapped]
    public string ExpenseHeadName { get; set; } = null!;
    [NotMapped]
    public string LocationDetailsName { get; set; } = null!;


    // Foriegn key relationships
    //[NotMapped]
    //public virtual ExpenseHead ExpenseHead { get; set; } = null!;
    //[NotMapped]
    //public virtual LocationDetails LocationDetails { get; set; } = null!;


}
