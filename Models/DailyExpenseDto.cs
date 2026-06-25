using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace TireInventory.Models;
public class DailyExpenseDto
{
    public long Id { get; set; }

    public long ExpenseHeadId { get; set; }

    public DateTime ExpenseDate { get; set; }

    public decimal? Amount { get; set; }

    public string? CheckNo { get; set; }

    public string? PayType { get; set; }

    public string? UserName { get; set; }

    public DateTime? SetDate { get; set; }
    public long? LocationDetailsId { get; set; }
    public string ExpenseHeadName { get; set; } = null!;
    public string LocationDetailsName { get; set; } = null!;


}
