namespace TireInventory.Models
{
    // ---------------------------------------------------------------------
    // FILTER / PERIOD
    // ---------------------------------------------------------------------

    /// <summary>
    /// Half-open date range [From, To). Always use "&gt;= From &amp;&amp; &lt; To" so that
    /// invoices stamped with a time component on the last day are not dropped.
    /// </summary>
    public class DashboardFilter
    {
        public DateTime From { get; set; }
        public DateTime To { get; set; }
        public long? LocationId { get; set; }

        public int DayCount => Math.Max(1, (int)(To - From).TotalDays);

        /// <summary>Same length window immediately before this one, for % deltas.</summary>
        public DashboardFilter PreviousPeriod()
        {
            var span = To - From;
            return new DashboardFilter
            {
                From = From - span,
                To = From,
                LocationId = LocationId
            };
        }

        public static DashboardFilter Resolve(string? period, DateTime? from, DateTime? to, long? locationId)
        {
            var today = DateTime.Today;
            DateTime f, t;

            switch ((period ?? "mtd").Trim().ToLowerInvariant())
            {
                case "today":
                    f = today; t = today.AddDays(1); break;
                case "yesterday":
                    f = today.AddDays(-1); t = today; break;
                case "wtd":
                    f = today.AddDays(-(int)today.DayOfWeek); t = today.AddDays(1); break;
                case "mtd":
                    f = new DateTime(today.Year, today.Month, 1); t = today.AddDays(1); break;
                case "lastmonth":
                    var fm = new DateTime(today.Year, today.Month, 1).AddMonths(-1);
                    f = fm; t = fm.AddMonths(1); break;
                case "ytd":
                    f = new DateTime(today.Year, 1, 1); t = today.AddDays(1); break;
                case "lastyear":
                    f = new DateTime(today.Year - 1, 1, 1); t = new DateTime(today.Year, 1, 1); break;
                case "last12m":
                    f = new DateTime(today.Year, today.Month, 1).AddMonths(-11); t = today.AddDays(1); break;
                case "custom":
                default:
                    f = from?.Date ?? new DateTime(today.Year, today.Month, 1);
                    t = (to?.Date ?? today).AddDays(1);
                    break;
            }

            return new DashboardFilter { From = f, To = t, LocationId = locationId };
        }
    }

    // ---------------------------------------------------------------------
    // KPI CARDS
    // ---------------------------------------------------------------------

    public class PeriodTotalsDto
    {
        public int InvoiceCount { get; set; }
        public decimal SubTotal { get; set; }
        public decimal Tax { get; set; }
        public decimal Labour { get; set; }
        public decimal Discount { get; set; }
        public decimal GrossSales { get; set; }
        public decimal NetSales { get; set; }
        public decimal PaidAmount { get; set; }
        public decimal Collected { get; set; }
        public decimal GrossProfit { get; set; }
        public int ItemsSold { get; set; }
        public int CustomerCount { get; set; }

        public decimal Outstanding => NetSales - PaidAmount;
        public decimal AverageInvoiceValue => InvoiceCount == 0 ? 0 : Math.Round(NetSales / InvoiceCount, 2);
        public decimal MarginPercent => NetSales == 0 ? 0 : Math.Round(GrossProfit / NetSales * 100m, 2);
    }

    public class KpiSummaryDto
    {
        public PeriodTotalsDto Current { get; set; } = new();
        public PeriodTotalsDto Previous { get; set; } = new();
        public Dictionary<string, decimal> ChangePercent { get; set; } = new();
        public DateTime From { get; set; }
        public DateTime To { get; set; }
    }

    // ---------------------------------------------------------------------
    // TIME SERIES
    // ---------------------------------------------------------------------

    public class YearlySalesDto
    {
        public int Year { get; set; }
        public decimal NetSales { get; set; }
        public decimal Collected { get; set; }
        public int InvoiceCount { get; set; }
    }

    public class MonthlySalesDto
    {
        public int Year { get; set; }
        public int Month { get; set; }
        public string Label { get; set; } = "";
        public decimal NetSales { get; set; }
        public decimal Collected { get; set; }
        public int InvoiceCount { get; set; }
    }

    public class DailySalesDto
    {
        public DateTime Date { get; set; }
        public decimal NetSales { get; set; }
        public int InvoiceCount { get; set; }
    }

    public class HeatmapCellDto
    {
        public int DayOfWeek { get; set; }          // 0 = Sunday
        public string DayName { get; set; } = "";
        public int Hour { get; set; }               // 0-23
        public decimal NetSales { get; set; }
        public int InvoiceCount { get; set; }
    }

    // ---------------------------------------------------------------------
    // BREAKDOWNS
    // ---------------------------------------------------------------------

    public class TopProductDto
    {
        public long? ItemId { get; set; }
        public string Description { get; set; } = "";
        public string? Brand { get; set; }
        public string? Size { get; set; }
        public string? Series { get; set; }
        public string? Department { get; set; }
        public int Quantity { get; set; }
        public decimal Revenue { get; set; }
        public decimal SharePercent { get; set; }
        public int? StockOnHand { get; set; }
    }

    public class TopCustomerDto
    {
        public string Phone { get; set; } = "";
        public string Name { get; set; } = "";
        public string? Email { get; set; }
        public int InvoiceCount { get; set; }
        public decimal Revenue { get; set; }
        public decimal Outstanding { get; set; }
        public DateTime LastPurchase { get; set; }
    }

    public class NameValueDto
    {
        public long? Id { get; set; }
        public string Name { get; set; } = "";
        public decimal Value { get; set; }
        public int Count { get; set; }
        public decimal SharePercent { get; set; }
    }

    public class PaymentCollectionDto
    {
        public long PaymentId { get; set; }
        public string PaymentName { get; set; } = "";
        public decimal Amount { get; set; }
        public int TransactionCount { get; set; }
        public decimal SharePercent { get; set; }
    }

    public class CustomerMixDto
    {
        public int NewCustomers { get; set; }
        public int ReturningCustomers { get; set; }
        public decimal NewCustomerRevenue { get; set; }
        public decimal ReturningCustomerRevenue { get; set; }
    }

    public class TirePositionDto
    {
        public int LeftFront { get; set; }
        public int RightFront { get; set; }
        public int LeftRear { get; set; }
        public int RightRear { get; set; }
    }

    // ---------------------------------------------------------------------
    // OPERATIONS / RISK
    // ---------------------------------------------------------------------

    public class OutstandingInvoiceDto
    {
        public long InvoiceId { get; set; }
        public DateTime InvoiceDate { get; set; }
        public string CustomerName { get; set; } = "";
        public string Phone { get; set; } = "";
        public decimal Total { get; set; }
        public decimal Paid { get; set; }
        public decimal Due { get; set; }
        public int AgeInDays { get; set; }
    }

    public class RecentInvoiceDto
    {
        public long InvoiceId { get; set; }
        public DateTime InvoiceDate { get; set; }
        public string CustomerName { get; set; } = "";
        public string Phone { get; set; } = "";
        public decimal Total { get; set; }
        public decimal Paid { get; set; }
        public string? PaymentInfo { get; set; }
        public int LineCount { get; set; }
    }

    public class LayawaySummaryDto
    {
        public int OpenCount { get; set; }
        public decimal OpenValue { get; set; }
        public decimal CollectedValue { get; set; }
        public decimal PendingValue { get; set; }
    }

    // ---------------------------------------------------------------------
    // INVENTORY
    // ---------------------------------------------------------------------

    public class InventorySummaryDto
    {
        public int SkuCount { get; set; }
        public int TotalUnits { get; set; }
        public decimal StockValueAtCost { get; set; }
        public int OutOfStockCount { get; set; }
        public int LowStockCount { get; set; }
    }

    public class StockItemDto
    {
        public long ItemId { get; set; }
        public string Description { get; set; } = "";
        public string? Brand { get; set; }
        public string? Size { get; set; }
        public int Quantity { get; set; }
        public decimal UnitCost { get; set; }
        public decimal StockValue { get; set; }
        public DateTime? LastSoldOn { get; set; }
        public int? DaysSinceLastSale { get; set; }
    }

    // ---------------------------------------------------------------------
    // AGGREGATE PAYLOAD
    // ---------------------------------------------------------------------

    public class DashboardOverviewDto
    {
        public KpiSummaryDto Kpi { get; set; } = new();
        public List<YearlySalesDto> YearlySales { get; set; } = new();
        public List<MonthlySalesDto> MonthlySales { get; set; } = new();
        public List<DailySalesDto> DailySales { get; set; } = new();
        public List<TopProductDto> TopProducts { get; set; } = new();
        public List<TopCustomerDto> TopCustomers { get; set; } = new();
        public List<PaymentCollectionDto> PaymentCollection { get; set; } = new();
        public List<NameValueDto> SalesByDepartment { get; set; } = new();
        public List<NameValueDto> SalesByBrand { get; set; } = new();
        public List<NameValueDto> SalesByLocation { get; set; } = new();
        public InventorySummaryDto Inventory { get; set; } = new();
        public List<OutstandingInvoiceDto> TopOutstanding { get; set; } = new();
        public List<RecentInvoiceDto> RecentInvoices { get; set; } = new();
    }
}
