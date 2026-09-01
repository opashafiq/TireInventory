using Microsoft.EntityFrameworkCore;
using System;
using System.Globalization;
using System.Linq.Expressions;
using TireInventory.Data;
using TireInventory.Models;

namespace TireInventory.Services
{
    /// <summary>
    /// Dashboard read model. All queries are AsNoTracking and projected to DTOs so
    /// nothing is materialised that the chart does not need.
    ///
    /// BEFORE YOU COMPILE — three things to align with your project:
    ///
    ///   1. DbContext type name .... replace "AppDbContext" below.
    ///   2. DbSet names ............ this file uses _context.InvoiceMaster,
    ///                               _context.InvoiceDetails, _context.ItemMaster,
    ///                               _context.InvoicePayments, _context.PaymentNames,
    ///                               _context.Departments, _context.Distributors,
    ///                               _context.LocationDetails.
    ///                               Rename to match your context (e.g. InvoiceMasters).
    ///   3. LocationDetails.Name ... see GetSalesByLocationAsync — the LocationDetails
    ///                               model was not supplied, so the display-name column
    ///                               is marked with a TODO.
    ///
    /// SQL Server 2014 notes:
    ///   * Every SUM is cast to a nullable type ("(decimal?)x") so an empty group
    ///     returns NULL and is coalesced to 0 instead of throwing on materialisation.
    ///   * DayOfWeek is NOT translated by EF Core, so the heatmap groups on
    ///     .Date + .Hour (both translate cleanly) and derives the weekday in memory.
    ///   * No DateOnly / TimeOnly anywhere — unsupported against SQL Server 2014.
    /// </summary>
    public class DashboardService : IDashboardService
    {
        private readonly ApplicationDBContext _context;

        public DashboardService(ApplicationDBContext context)
        {
            _context = context;
        }

        // =================================================================
        // BUSINESS-RULE CONSTANTS — change these in ONE place, not 20
        // =================================================================

        /// <summary>
        /// Net sales value of an invoice. Currently tbim_AdjTotal (post-adjustment).
        /// Switch to tbim_Total if AdjTotal is not populated on historical rows.
        /// </summary>
        private static readonly Expression<Func<InvoiceMaster, decimal>> NetSalesOf =
            i => i.tbim_AdjTotal;

        private static readonly Func<InvoiceMaster, decimal> NetSalesOfCompiled = NetSalesOf.Compile();

        /// <summary>
        /// ASSUMPTION: ItemMaster.tbim_Code is the unit COST. Every profit and
        /// stock-value figure depends on this. Confirm before showing margin to a client.
        /// </summary>
        private const string CostFieldNote = "ItemMaster.tbim_Code treated as unit cost";

        // =================================================================
        // SHARED BASE QUERIES
        // =================================================================

        private IQueryable<InvoiceMaster> Invoices(DashboardFilter f)
        {
            var q = _context.InvoiceMasters
                .AsNoTracking()
                .Where(i => i.tbim_InvDate >= f.From && i.tbim_InvDate < f.To);

            if (f.LocationId.HasValue)
                q = q.Where(i => i.tbim_LocationDetailsId == f.LocationId.Value);

            // TODO: when the InvoiceRefundMaster model is available, exclude or net
            // refunded invoices here, e.g. .Where(i => i.tbim_RefundType == null)
            return q;
        }

        /// <summary>Invoice lines filtered by the PARENT invoice date.</summary>
        private IQueryable<InvoiceDetails> Lines(DashboardFilter f)
        {
            var q = _context.InvoiceDetails
                .AsNoTracking()
                .Where(d => d.tbid_Invoice!.tbim_InvDate >= f.From
                         && d.tbid_Invoice.tbim_InvDate < f.To);

            if (f.LocationId.HasValue)
                q = q.Where(d => d.tbid_Invoice!.tbim_LocationDetailsId == f.LocationId.Value);

            return q;
        }

        /// <summary>
        /// Payments filtered by PAYMENT date, not invoice date — collection reporting
        /// must follow the cash, otherwise a January invoice paid in March lands in January.
        /// </summary>
        private IQueryable<InvoicePayments> Payments(DashboardFilter f)
        {
            var q = _context.InvoicePayments
                .AsNoTracking()
                .Where(p => p.tbip_Date >= f.From && p.tbip_Date < f.To);

            if (f.LocationId.HasValue)
                q = q.Where(p => p.tbip_Invoice.tbim_LocationDetailsId == f.LocationId.Value);

            return q;
        }

        private static decimal Pct(decimal current, decimal previous)
        {
            if (previous == 0) return current == 0 ? 0 : 100m;
            return Math.Round((current - previous) / Math.Abs(previous) * 100m, 2);
        }

        private static void ApplyShares(IEnumerable<NameValueDto> rows)
        {
            var total = rows.Sum(r => r.Value);
            if (total == 0) return;
            foreach (var r in rows)
                r.SharePercent = Math.Round(r.Value / total * 100m, 2);
        }

        // =================================================================
        // 1. KPI SUMMARY  (cards, with previous-period deltas)
        // =================================================================

        public async Task<KpiSummaryDto> GetKpiSummaryAsync(DashboardFilter f, CancellationToken ct = default)
        {
            var current = await AggregatePeriodAsync(f, ct);
            var previous = await AggregatePeriodAsync(f.PreviousPeriod(), ct);

            return new KpiSummaryDto
            {
                From = f.From,
                To = f.To,
                Current = current,
                Previous = previous,
                ChangePercent = new Dictionary<string, decimal>
                {
                    ["netSales"] = Pct(current.NetSales, previous.NetSales),
                    ["grossSales"] = Pct(current.GrossSales, previous.GrossSales),
                    ["invoiceCount"] = Pct(current.InvoiceCount, previous.InvoiceCount),
                    ["averageInvoice"] = Pct(current.AverageInvoiceValue, previous.AverageInvoiceValue),
                    ["collected"] = Pct(current.Collected, previous.Collected),
                    ["outstanding"] = Pct(current.Outstanding, previous.Outstanding),
                    ["grossProfit"] = Pct(current.GrossProfit, previous.GrossProfit),
                    ["itemsSold"] = Pct(current.ItemsSold, previous.ItemsSold),
                    ["customerCount"] = Pct(current.CustomerCount, previous.CustomerCount),
                    ["tax"] = Pct(current.Tax, previous.Tax),
                    ["labour"] = Pct(current.Labour, previous.Labour),
                    ["discount"] = Pct(current.Discount, previous.Discount)
                }
            };
        }

        private async Task<PeriodTotalsDto> AggregatePeriodAsync(DashboardFilter f, CancellationToken ct)
        {
            // One pass over the invoice header for all money columns.
            var totals = await Invoices(f)
                .GroupBy(_ => 1)
                .Select(g => new PeriodTotalsDto
                {
                    InvoiceCount = g.Count(),
                    SubTotal = g.Sum(x => (decimal?)x.tbim_SubTotal) ?? 0m,
                    Tax = g.Sum(x => (decimal?)x.tbim_SaleTax) ?? 0m,
                    Labour = g.Sum(x => (decimal?)x.tbim_Labour) ?? 0m,
                    Discount = g.Sum(x => (decimal?)x.tbim_DisAmt) ?? 0m,
                    GrossSales = g.Sum(x => (decimal?)x.tbim_Total) ?? 0m,
                    NetSales = g.Sum(x => (decimal?)x.tbim_AdjTotal) ?? 0m,
                    PaidAmount = g.Sum(x => (decimal?)x.tbim_PaidAmt) ?? 0m
                })
                .FirstOrDefaultAsync(ct) ?? new PeriodTotalsDto();

            totals.ItemsSold = await Lines(f).SumAsync(d => (int?)d.tbid_Qty, ct) ?? 0;

            totals.CustomerCount = await Invoices(f)
                .Select(i => i.tbim_Phone)
                .Distinct()
                .CountAsync(ct);

            totals.Collected = await Payments(f).SumAsync(p => (decimal?)p.tbip_PayAmt, ct) ?? 0m;

            totals.GrossProfit = await GetGrossProfitAsync(f, ct);

            return totals;
        }

        /// <summary>
        /// Approximate gross profit: line revenue minus (qty x CURRENT item cost).
        /// Cost is not snapshotted on the invoice line, so this drifts as costs change.
        /// Add a tbid_UnitCost column to InvoiceDetails to make it exact.
        /// </summary>
        private async Task<decimal> GetGrossProfitAsync(DashboardFilter f, CancellationToken ct)
        {
            var query =
                from d in Lines(f)
                join it in _context.ItemMasters.AsNoTracking()
                    on d.tbid_ItemId equals (long?)it.Id
                select (decimal?)(d.tbid_LineTotal - (d.tbid_Qty * it.tbim_Code));

            return await query.SumAsync(x => x, ct) ?? 0m;
        }

        // =================================================================
        // 2. YEARLY SALES  (client requirement #1)
        // =================================================================

        public async Task<List<YearlySalesDto>> GetYearlySalesAsync(int years, long? locationId, CancellationToken ct = default)
        {
            if (years <= 0) years = 5;

            var startYear = DateTime.Today.Year - (years - 1);
            var from = new DateTime(startYear, 1, 1);
            var to = new DateTime(DateTime.Today.Year + 1, 1, 1);
            var f = new DashboardFilter { From = from, To = to, LocationId = locationId };

            var sales = await Invoices(f)
                .GroupBy(i => i.tbim_InvDate.Year)
                .Select(g => new
                {
                    Year = g.Key,
                    NetSales = g.Sum(x => (decimal?)x.tbim_AdjTotal) ?? 0m,
                    InvoiceCount = g.Count()
                })
                .ToListAsync(ct);

            var collections = await Payments(f)
                .GroupBy(p => p.tbip_Date!.Value.Year)
                .Select(g => new { Year = g.Key, Amount = g.Sum(x => (decimal?)x.tbip_PayAmt) ?? 0m })
                .ToListAsync(ct);

            // Fill missing years so the chart axis never has holes.
            var result = new List<YearlySalesDto>();
            for (var y = startYear; y <= DateTime.Today.Year; y++)
            {
                var s = sales.FirstOrDefault(x => x.Year == y);
                var c = collections.FirstOrDefault(x => x.Year == y);
                result.Add(new YearlySalesDto
                {
                    Year = y,
                    NetSales = s?.NetSales ?? 0m,
                    InvoiceCount = s?.InvoiceCount ?? 0,
                    Collected = c?.Amount ?? 0m
                });
            }

            return result;
        }

        // =================================================================
        // 3. MONTHLY SALES, LAST 12 MONTHS  (client requirement #2)
        // =================================================================

        public async Task<List<MonthlySalesDto>> GetMonthlySalesAsync(int months, long? locationId, CancellationToken ct = default)
        {
            if (months <= 0) months = 12;

            var today = DateTime.Today;
            var from = new DateTime(today.Year, today.Month, 1).AddMonths(-(months - 1));
            var to = new DateTime(today.Year, today.Month, 1).AddMonths(1);
            var f = new DashboardFilter { From = from, To = to, LocationId = locationId };

            var sales = await Invoices(f)
                .GroupBy(i => new { i.tbim_InvDate.Year, i.tbim_InvDate.Month })
                .Select(g => new
                {
                    g.Key.Year,
                    g.Key.Month,
                    NetSales = g.Sum(x => (decimal?)x.tbim_AdjTotal) ?? 0m,
                    InvoiceCount = g.Count()
                })
                .ToListAsync(ct);

            var collections = await Payments(f)
                .GroupBy(p => new { p.tbip_Date!.Value.Year, p.tbip_Date!.Value.Month })
                .Select(g => new
                {
                    g.Key.Year,
                    g.Key.Month,
                    Amount = g.Sum(x => (decimal?)x.tbip_PayAmt) ?? 0m
                })
                .ToListAsync(ct);

            var result = new List<MonthlySalesDto>();
            for (var i = 0; i < months; i++)
            {
                var cursor = from.AddMonths(i);
                var s = sales.FirstOrDefault(x => x.Year == cursor.Year && x.Month == cursor.Month);
                var c = collections.FirstOrDefault(x => x.Year == cursor.Year && x.Month == cursor.Month);

                result.Add(new MonthlySalesDto
                {
                    Year = cursor.Year,
                    Month = cursor.Month,
                    Label = cursor.ToString("MMM yyyy", CultureInfo.InvariantCulture),
                    NetSales = s?.NetSales ?? 0m,
                    InvoiceCount = s?.InvoiceCount ?? 0,
                    Collected = c?.Amount ?? 0m
                });
            }

            return result;
        }

        // =================================================================
        // 4. DAILY SALES  (current-month trend line)
        // =================================================================

        public async Task<List<DailySalesDto>> GetDailySalesAsync(DashboardFilter f, CancellationToken ct = default)
        {
            var rows = await Invoices(f)
                .GroupBy(i => i.tbim_InvDate.Date)
                .Select(g => new DailySalesDto
                {
                    Date = g.Key,
                    NetSales = g.Sum(x => (decimal?)x.tbim_AdjTotal) ?? 0m,
                    InvoiceCount = g.Count()
                })
                .ToListAsync(ct);

            var byDate = rows.ToDictionary(r => r.Date);
            var result = new List<DailySalesDto>();
            for (var d = f.From.Date; d < f.To.Date; d = d.AddDays(1))
            {
                result.Add(byDate.TryGetValue(d, out var hit)
                    ? hit
                    : new DailySalesDto { Date = d, NetSales = 0m, InvoiceCount = 0 });
            }

            return result;
        }

        // =================================================================
        // 5. SALES HEATMAP  (day of week x hour — staffing insight)
        // =================================================================

        public async Task<List<HeatmapCellDto>> GetSalesHeatmapAsync(DashboardFilter f, CancellationToken ct = default)
        {
            // DayOfWeek is not SQL-translatable; group on Date + Hour and fold in memory.
            var raw = await Invoices(f)
                .GroupBy(i => new { Day = i.tbim_InvDate.Date, Hour = i.tbim_InvDate.Hour })
                .Select(g => new
                {
                    g.Key.Day,
                    g.Key.Hour,
                    NetSales = g.Sum(x => (decimal?)x.tbim_AdjTotal) ?? 0m,
                    InvoiceCount = g.Count()
                })
                .ToListAsync(ct);

            return raw
                .GroupBy(x => new { Dow = (int)x.Day.DayOfWeek, x.Hour })
                .Select(g => new HeatmapCellDto
                {
                    DayOfWeek = g.Key.Dow,
                    DayName = CultureInfo.InvariantCulture.DateTimeFormat.GetDayName((DayOfWeek)g.Key.Dow),
                    Hour = g.Key.Hour,
                    NetSales = g.Sum(x => x.NetSales),
                    InvoiceCount = g.Sum(x => x.InvoiceCount)
                })
                .OrderBy(x => x.DayOfWeek).ThenBy(x => x.Hour)
                .ToList();
        }

        // =================================================================
        // 6. CARD-WISE COLLECTION  (client requirement #4)
        // =================================================================

        public async Task<List<PaymentCollectionDto>> GetCollectionByPaymentMethodAsync(DashboardFilter f, CancellationToken ct = default)
        {
            var rows = await Payments(f)
                .GroupBy(p => new
                {
                    p.tbip_PaymentId,
                    // tbip_Payment is a nullable nav with [ForeignKey], so EF emits a LEFT JOIN.
                    Name = p.tbip_Payment != null ? p.tbip_Payment.tbpn_PaymentName : p.tbip_PaymentType
                })
                .Select(g => new PaymentCollectionDto
                {
                    PaymentId = g.Key.tbip_PaymentId,
                    PaymentName = g.Key.Name ?? "Unspecified",
                    Amount = g.Sum(x => (decimal?)x.tbip_PayAmt) ?? 0m,
                    TransactionCount = g.Count()
                })
                .OrderByDescending(x => x.Amount)
                .ToListAsync(ct);

            var total = rows.Sum(r => r.Amount);
            if (total != 0)
                foreach (var r in rows)
                    r.SharePercent = Math.Round(r.Amount / total * 100m, 2);

            return rows;
        }

        // =================================================================
        // 7. LAYAWAY SUMMARY
        // =================================================================

        public async Task<LayawaySummaryDto> GetLayawaySummaryAsync(DashboardFilter f, CancellationToken ct = default)
        {
            var summary = await Invoices(f)
                .Where(i => i.tbim_LaywayNo != null)
                .GroupBy(_ => 1)
                .Select(g => new LayawaySummaryDto
                {
                    OpenCount = g.Count(),
                    OpenValue = g.Sum(x => (decimal?)x.tbim_AdjTotal) ?? 0m,
                    CollectedValue = g.Sum(x => (decimal?)x.tbim_PaidAmt) ?? 0m
                })
                .FirstOrDefaultAsync(ct) ?? new LayawaySummaryDto();

            summary.PendingValue = summary.OpenValue - summary.CollectedValue;
            return summary;
        }

        // =================================================================
        // 8. TOP PRODUCTS BY VALUE  (client requirement #3)
        // =================================================================

        public async Task<List<TopProductDto>> GetTopProductsByValueAsync(int top, DashboardFilter f, CancellationToken ct = default)
            => await GetTopProductsAsync(top, f, byQuantity: false, ct);

        public async Task<List<TopProductDto>> GetTopProductsByQuantityAsync(int top, DashboardFilter f, CancellationToken ct = default)
            => await GetTopProductsAsync(top, f, byQuantity: true, ct);

        private async Task<List<TopProductDto>> GetTopProductsAsync(int top, DashboardFilter f, bool byQuantity, CancellationToken ct)
        {
            if (top <= 0) top = 10;

            // 1. InvoiceDetails already carries brand/size/series/department snapshots,
            // so no join to ItemMaster is needed for the chart itself.
            var q = Lines(f)
                .GroupBy(d => new
                {
                    d.tbid_ItemId,
                    d.tbid_Brand,
                    d.tbid_Size,
                    d.tbid_Series,
                    d.tbid_DepartmentName
                })
                .Select(g => new TopProductDto
                {
                    ItemId = g.Key.tbid_ItemId,
                    Brand = g.Key.tbid_Brand,
                    Size = g.Key.tbid_Size,
                    Series = g.Key.tbid_Series,
                    Department = g.Key.tbid_DepartmentName,
                    Quantity = g.Sum(x => (int?)x.tbid_Qty) ?? 0,
                    Revenue = g.Sum(x => (decimal?)x.tbid_LineTotal) ?? 0m
                });

            q = byQuantity
                ? q.OrderByDescending(x => x.Quantity)
                : q.OrderByDescending(x => x.Revenue);

            var rows = await q.Take(top).ToListAsync(ct);

            if (rows.Count == 0) return rows;

            // 2. Period total for share %, computed server-side over the whole period.
            var periodRevenue = await Lines(f).SumAsync(d => (decimal?)d.tbid_LineTotal, ct) ?? 0m;

            // 3. Attach live stock using raw SQL with explicit semicolon termination
            var itemIds = rows
                .Where(r => r.ItemId.HasValue)
                .Select(r => r.ItemId!.Value)
                .Distinct()
                .ToList();

            var stock = new Dictionary<long, int>();
            if (itemIds.Count > 0)
            {
                // Generate comma-separated parameters dynamically or use string formatting safely for IN clause
                var idList = string.Join(",", itemIds);

                // Explicitly ending with a semicolon prevents the parser error
                var stockItems = await _context.ItemMasters
                    .FromSqlRaw($"SELECT * FROM tbl_ItemMaster WHERE Id IN ({idList})")
                    .AsNoTracking()
                    .Select(i => new { i.Id, Qty = i.tbim_Qty })
                    .ToListAsync(ct);

                stock = stockItems.ToDictionary(x => x.Id, x => x.Qty);
            }

            // 4. Populate formatted fields, share percentage, and live stock values
            foreach (var r in rows)
            {
                r.Description = string.Join(" ", new[] { r.Brand, r.Size, r.Series }
                    .Where(s => !string.IsNullOrWhiteSpace(s)));

                if (string.IsNullOrWhiteSpace(r.Description))
                    r.Description = r.Department ?? $"Item #{r.ItemId}";

                if (periodRevenue != 0)
                    r.SharePercent = Math.Round(r.Revenue / periodRevenue * 100m, 2);

                if (r.ItemId.HasValue && stock.TryGetValue(r.ItemId.Value, out var qty))
                    r.StockOnHand = qty;
            }

            return rows;
        }

        // =================================================================
        // 9. SALES BY DEPARTMENT / BRAND / DISTRIBUTOR
        // =================================================================

        public async Task<List<NameValueDto>> GetSalesByDepartmentAsync(DashboardFilter f, CancellationToken ct = default)
        {
            var rows = await Lines(f)
                .GroupBy(d => new { d.tbid_ItemCategory, d.tbid_DepartmentName })
                .Select(g => new NameValueDto
                {
                    Id = g.Key.tbid_ItemCategory,
                    Name = g.Key.tbid_DepartmentName ?? "",
                    Value = g.Sum(x => (decimal?)x.tbid_LineTotal) ?? 0m,
                    Count = g.Sum(x => (int?)x.tbid_Qty) ?? 0
                })
                .OrderByDescending(x => x.Value)
                .ToListAsync(ct);

            // Resolve any blank snapshot names from the Departments master.
            var missing = rows.Where(r => string.IsNullOrWhiteSpace(r.Name) && r.Id.HasValue)
                              .Select(r => r.Id!.Value).Distinct().ToList();
            if (missing.Count > 0)
            {
                var names = await _context.Departments.AsNoTracking()
                    .Where(d => missing.Contains(d.Id))
                    .ToDictionaryAsync(d => d.Id, d => d.Tbid_DepartmentName, ct);

                foreach (var r in rows.Where(r => string.IsNullOrWhiteSpace(r.Name)))
                    r.Name = r.Id.HasValue && names.TryGetValue(r.Id.Value, out var n) ? n : "Uncategorised";
            }

            ApplyShares(rows);
            return rows;
        }

        public async Task<List<NameValueDto>> GetSalesByBrandAsync(int top, DashboardFilter f, CancellationToken ct = default)
        {
            if (top <= 0) top = 10;

            var rows = await Lines(f)
                .Where(d => d.tbid_Brand != null && d.tbid_Brand != "")
                .GroupBy(d => d.tbid_Brand)
                .Select(g => new NameValueDto
                {
                    Name = g.Key!,
                    Value = g.Sum(x => (decimal?)x.tbid_LineTotal) ?? 0m,
                    Count = g.Sum(x => (int?)x.tbid_Qty) ?? 0
                })
                .OrderByDescending(x => x.Value)
                .Take(top)
                .ToListAsync(ct);

            ApplyShares(rows);
            return rows;
        }

        public async Task<List<NameValueDto>> GetSalesByDistributorAsync(int top, DashboardFilter f, CancellationToken ct = default)
        {
            if (top <= 0) top = 10;

            var rows = await Lines(f)
                .Where(d => d.tbid_DistributorId != null)
                .GroupBy(d => new { d.tbid_DistributorId, d.tbid_DistributorName })
                .Select(g => new NameValueDto
                {
                    Id = g.Key.tbid_DistributorId,
                    Name = g.Key.tbid_DistributorName ?? "",
                    Value = g.Sum(x => (decimal?)x.tbid_LineTotal) ?? 0m,
                    Count = g.Sum(x => (int?)x.tbid_Qty) ?? 0
                })
                .OrderByDescending(x => x.Value)
                .Take(top)
                .ToListAsync(ct);

            var missing = rows.Where(r => string.IsNullOrWhiteSpace(r.Name) && r.Id.HasValue)
                              .Select(r => (int)r.Id!.Value).Distinct().ToList();
            if (missing.Count > 0)
            {
                var names = await _context.Distributors.AsNoTracking()
                    .Where(d => missing.Contains(d.Id))
                    .ToDictionaryAsync(d => (long)d.Id, d => d.Name, ct);

                foreach (var r in rows.Where(r => string.IsNullOrWhiteSpace(r.Name)))
                    r.Name = r.Id.HasValue && names.TryGetValue(r.Id.Value, out var n) ? n : "Unknown";
            }

            ApplyShares(rows);
            return rows;
        }

        // =================================================================
        // 10. TOP CUSTOMERS  (client requirement #5)
        // =================================================================

        public async Task<List<TopCustomerDto>> GetTopCustomersAsync(int top, DashboardFilter f, CancellationToken ct = default)
        {
            if (top <= 0) top = 10;

            // Phone is the stable customer key; name can be typed differently each visit,
            // so take the most recent spelling via the max invoice date row.
            return await Invoices(f)
                .GroupBy(i => i.tbim_Phone)
                .Select(g => new TopCustomerDto
                {
                    Phone = g.Key,
                    Name = g.OrderByDescending(x => x.tbim_InvDate).Select(x => x.tbim_Name).FirstOrDefault() ?? "",
                    Email = g.OrderByDescending(x => x.tbim_InvDate).Select(x => x.tbim_EmailAddress).FirstOrDefault(),
                    InvoiceCount = g.Count(),
                    Revenue = g.Sum(x => (decimal?)x.tbim_AdjTotal) ?? 0m,
                    Outstanding = (g.Sum(x => (decimal?)x.tbim_AdjTotal) ?? 0m) - (g.Sum(x => (decimal?)x.tbim_PaidAmt) ?? 0m),
                    LastPurchase = g.Max(x => x.tbim_InvDate)
                })
                .OrderByDescending(x => x.Revenue)
                .Take(top)
                .ToListAsync(ct);
        }

        // =================================================================
        // 11. NEW vs RETURNING CUSTOMERS
        // =================================================================

        public async Task<CustomerMixDto> GetCustomerMixAsync(DashboardFilter f, CancellationToken ct = default)
        {
            // Customers active in the period, with their revenue in the period.
            var inPeriod = await Invoices(f)
                .GroupBy(i => i.tbim_Phone)
                .Select(g => new
                {
                    Phone = g.Key,
                    Revenue = g.Sum(x => (decimal?)x.tbim_AdjTotal) ?? 0m
                })
                .ToListAsync(ct);

            if (inPeriod.Count == 0) return new CustomerMixDto();

            var phones = inPeriod
                .Where(x => !string.IsNullOrEmpty(x.Phone))
                .Select(x => x.Phone!)
                .Distinct()
                .ToList();

            var firstSeen = new Dictionary<string, DateTime>();

            if (phones.Count > 0)
            {
                // -------------------------------------------------------------
                // FIX: Formats phone strings for SQL IN clause & appends an 
                // explicit trailing semicolon ';' to prevent the 'WITH' CTE error
                // -------------------------------------------------------------
                var phoneList = string.Join(",", phones.Select(p => $"'{p.Replace("'", "''")}'"));

                var firstSeenItems = await _context.InvoiceMasters
                    .FromSqlRaw($"SELECT * FROM tbl_Invoice_Master WHERE tbim_Phone IN ({phoneList})")
                    .AsNoTracking()
                    .Where(i => !string.IsNullOrEmpty(i.tbim_Phone))
                    .GroupBy(i => i.tbim_Phone!)
                    .Select(g => new
                    {
                        Phone = g.Key,
                        First = g.Min(x => (DateTime?)x.tbim_InvDate)
                    })
                    .ToListAsync(ct);

                firstSeen = firstSeenItems
                    .Where(x => x.First.HasValue)
                    .ToDictionary(x => x.Phone, x => x.First!.Value);
            }

            var mix = new CustomerMixDto();
            foreach (var c in inPeriod)
            {
                var isNew = c.Phone != null
                    && firstSeen.TryGetValue(c.Phone, out var first)
                    && first >= f.From;

                if (isNew)
                {
                    mix.NewCustomers++;
                    mix.NewCustomerRevenue += c.Revenue;
                }
                else
                {
                    mix.ReturningCustomers++;
                    mix.ReturningCustomerRevenue += c.Revenue;
                }
            }

            return mix;
        }

        // =================================================================
        // 12. VEHICLE INSIGHTS
        // =================================================================

        public async Task<List<NameValueDto>> GetTopVehicleMakesAsync(int top, DashboardFilter f, CancellationToken ct = default)
        {
            if (top <= 0) top = 10;

            var rows = await Invoices(f)
                .Where(i => i.tbim_VehicleMake != null && i.tbim_VehicleMake != "")
                .GroupBy(i => i.tbim_VehicleMake)
                .Select(g => new NameValueDto
                {
                    Name = g.Key!,
                    Value = g.Sum(x => (decimal?)x.tbim_AdjTotal) ?? 0m,
                    Count = g.Count()
                })
                .OrderByDescending(x => x.Value)
                .Take(top)
                .ToListAsync(ct);

            ApplyShares(rows);
            return rows;
        }

        public async Task<TirePositionDto> GetTirePositionDemandAsync(DashboardFilter f, CancellationToken ct = default)
        {
            return await Invoices(f)
                .GroupBy(_ => 1)
                .Select(g => new TirePositionDto
                {
                    LeftFront = g.Sum(x => x.tbim_Left_Front ? 1 : 0),
                    RightFront = g.Sum(x => x.tbim_Right_Front ? 1 : 0),
                    LeftRear = g.Sum(x => x.tbim_Left_Rear ? 1 : 0),
                    RightRear = g.Sum(x => x.tbim_Right_Rear ? 1 : 0)
                })
                .FirstOrDefaultAsync(ct) ?? new TirePositionDto();
        }

        // =================================================================
        // 13. SALES BY LOCATION (branch comparison)
        // =================================================================

        public async Task<List<NameValueDto>> GetSalesByLocationAsync(DashboardFilter f, CancellationToken ct = default)
        {
            var rows = await Invoices(f)
                .GroupBy(i => i.tbim_LocationDetailsId)
                .Select(g => new NameValueDto
                {
                    Id = g.Key,
                    Value = g.Sum(x => (decimal?)x.tbim_AdjTotal) ?? 0m,
                    Count = g.Count()
                })
                .OrderByDescending(x => x.Value)
                .ToListAsync(ct);

            if (rows.Count == 0) return rows;

            var ids = rows
                .Where(r => r.Id.HasValue)
                .Select(r => r.Id!.Value)
                .Distinct()
                .ToList();

            if (ids.Count > 0)
            {
                // -------------------------------------------------------------
                // FIX: Executes raw SQL with an explicit trailing semicolon ';' 
                // to terminate SQL batch parser state and bypass the 'WITH' error
                // -------------------------------------------------------------
                var idList = string.Join(",", ids);

                var locationItems = await _context.LocationDetails
                    .FromSqlRaw($"SELECT * FROM tbl_BO_LocationDetails WHERE Id IN ({idList})")
                    .AsNoTracking()
                    .Select(l => new { l.Id, l.tbld_LocationName })
                    .ToListAsync(ct);

                var names = locationItems.ToDictionary(x => x.Id, x => x.tbld_LocationName);

                foreach (var r in rows)
                {
                    r.Name = r.Id.HasValue && names.TryGetValue(r.Id.Value, out var n) ? n : "Unassigned";
                }
            }

            ApplyShares(rows);
            return rows;
        }

        // =================================================================
        // 14. OUTSTANDING / RECENT INVOICES
        // =================================================================

        public async Task<List<OutstandingInvoiceDto>> GetTopOutstandingInvoicesAsync(int top, DashboardFilter f, CancellationToken ct = default)
        {
            if (top <= 0) top = 10;
            var today = DateTime.Today;

            return await Invoices(f)
                .Where(i => i.tbim_AdjTotal - i.tbim_PaidAmt > 0)
                .OrderByDescending(i => i.tbim_AdjTotal - i.tbim_PaidAmt)
                .Take(top)
                .Select(i => new OutstandingInvoiceDto
                {
                    InvoiceId = i.Id,
                    InvoiceDate = i.tbim_InvDate,
                    CustomerName = i.tbim_Name,
                    Phone = i.tbim_Phone,
                    Total = i.tbim_AdjTotal,
                    Paid = i.tbim_PaidAmt,
                    Due = i.tbim_AdjTotal - i.tbim_PaidAmt,
                    AgeInDays = EF.Functions.DateDiffDay(i.tbim_InvDate, today)
                })
                .ToListAsync(ct);
        }

        public async Task<List<RecentInvoiceDto>> GetRecentInvoicesAsync(int top, long? locationId, CancellationToken ct = default)
        {
            if (top <= 0) top = 10;

            var q = _context.InvoiceMasters.AsNoTracking().AsQueryable();
            if (locationId.HasValue)
                q = q.Where(i => i.tbim_LocationDetailsId == locationId.Value);

            return await q
                .OrderByDescending(i => i.tbim_InvDate).ThenByDescending(i => i.Id)
                .Take(top)
                .Select(i => new RecentInvoiceDto
                {
                    InvoiceId = i.Id,
                    InvoiceDate = i.tbim_InvDate,
                    CustomerName = i.tbim_Name,
                    Phone = i.tbim_Phone,
                    Total = i.tbim_AdjTotal,
                    Paid = i.tbim_PaidAmt,
                    PaymentInfo = i.tbim_PayInfo,
                    LineCount = i.InvoiceDetails.Count()
                })
                .ToListAsync(ct);
        }

        // =================================================================
        // 15. INVENTORY
        // =================================================================

        public async Task<InventorySummaryDto> GetInventorySummaryAsync(long? locationId, int lowStockThreshold = 4, CancellationToken ct = default)
        {
            var q = _context.ItemMasters.AsNoTracking()
                .Where(i => i.tbim_ThrashDate == null);   // exclude trashed/retired SKUs

            if (locationId.HasValue)
                q = q.Where(i => i.tbim_LocationId == locationId.Value);

            return await q
                .GroupBy(_ => 1)
                .Select(g => new InventorySummaryDto
                {
                    SkuCount = g.Count(),
                    TotalUnits = g.Sum(x => (int?)x.tbim_Qty) ?? 0,
                    StockValueAtCost = g.Sum(x => (decimal?)(x.tbim_Qty * x.tbim_Code)) ?? 0m,
                    OutOfStockCount = g.Sum(x => x.tbim_Qty <= 0 ? 1 : 0),
                    LowStockCount = g.Sum(x => x.tbim_Qty > 0 && x.tbim_Qty <= lowStockThreshold ? 1 : 0)
                })
                .FirstOrDefaultAsync(ct) ?? new InventorySummaryDto();
        }

        public async Task<List<StockItemDto>> GetLowStockItemsAsync(int threshold, int top, long? locationId, CancellationToken ct = default)
        {
            if (threshold <= 0) threshold = 4;
            if (top <= 0) top = 20;

            var q = _context.ItemMasters.AsNoTracking()
                .Where(i => i.tbim_ThrashDate == null && i.tbim_Qty <= threshold);

            if (locationId.HasValue)
                q = q.Where(i => i.tbim_LocationId == locationId.Value);

            return await q
                .OrderBy(i => i.tbim_Qty)
                .Take(top)
                .Select(i => new StockItemDto
                {
                    ItemId = i.Id,
                    Brand = i.tbim_Brand,
                    Size = i.tbim_Size,
                    Description = i.tbim_Brand + " " + i.tbim_Size + " " + (i.tbim_Series ?? ""),
                    Quantity = i.tbim_Qty,
                    UnitCost = i.tbim_Code,
                    StockValue = i.tbim_Qty * i.tbim_Code
                })
                .ToListAsync(ct);
        }

        /// <summary>
        /// Items still on hand that have not sold in the last N days — cash frozen on shelves.
        /// </summary>
        public async Task<List<StockItemDto>> GetDeadStockAsync(int days, int top, long? locationId, CancellationToken ct = default)
        {
            if (days <= 0) days = 180;
            if (top <= 0) top = 20;

            var since = DateTime.Today.AddDays(-days);

            var soldItemIds = _context.InvoiceDetails.AsNoTracking()
                .Where(d => d.tbid_ItemId != null && d.tbid_Invoice!.tbim_InvDate >= since)
                .Select(d => d.tbid_ItemId!.Value);

            var q = _context.ItemMasters.AsNoTracking()
                .Where(i => i.tbim_ThrashDate == null
                         && i.tbim_Qty > 0
                         && !soldItemIds.Contains(i.Id));

            if (locationId.HasValue)
                q = q.Where(i => i.tbim_LocationId == locationId.Value);

            var rows = await q
                .OrderByDescending(i => i.tbim_Qty * i.tbim_Code)
                .Take(top)
                .Select(i => new StockItemDto
                {
                    ItemId = i.Id,
                    Brand = i.tbim_Brand,
                    Size = i.tbim_Size,
                    Description = i.tbim_Brand + " " + i.tbim_Size + " " + (i.tbim_Series ?? ""),
                    Quantity = i.tbim_Qty,
                    UnitCost = i.tbim_Code,
                    StockValue = i.tbim_Qty * i.tbim_Code
                })
                .ToListAsync(ct);

            if (rows.Count == 0) return rows;

            // Attach genuine last-sold date using raw SQL with explicit semicolon termination
            var ids = rows.Select(r => r.ItemId).Distinct().ToList();
            if (ids.Count > 0)
            {
                var idList = string.Join(",", ids);

                var lastSoldItems = await _context.InvoiceDetails
                    .FromSqlRaw($"SELECT * FROM tbl_Invoice_Details WHERE tbid_ItemId IN ({idList})")
                    .AsNoTracking()
                    .Where(d => d.tbid_ItemId != null)
                    .GroupBy(d => d.tbid_ItemId!.Value)
                    .Select(g => new
                    {
                        ItemId = g.Key,
                        // Cast to DateTime? so null checking works safely across EF Core translation
                        Last = g.Max(x => (DateTime?)x.tbid_Invoice!.tbim_InvDate)
                    })
                    .ToListAsync(ct);

                var lastSold = lastSoldItems
                    .Where(x => x.Last.HasValue)
                    .ToDictionary(x => x.ItemId, x => x.Last!.Value);

                foreach (var r in rows)
                {
                    if (!lastSold.TryGetValue(r.ItemId, out var last)) continue;
                    r.LastSoldOn = last;
                    r.DaysSinceLastSale = (int)(DateTime.Today - last.Date).TotalDays;
                }
            }

            return rows;
        }

        // =================================================================
        // 16. OVERVIEW — one call for the first paint
        // =================================================================

        public async Task<DashboardOverviewDto> GetOverviewAsync(DashboardFilter f, CancellationToken ct = default)
        {
            // Sequential on purpose: a single scoped DbContext is NOT thread-safe,
            // so do NOT wrap these in Task.WhenAll unless you resolve a
            // separate context per call via IDbContextFactory<AppDbContext>.
            return new DashboardOverviewDto
            {
                Kpi = await GetKpiSummaryAsync(f, ct),
                YearlySales = await GetYearlySalesAsync(5, f.LocationId, ct),
                MonthlySales = await GetMonthlySalesAsync(12, f.LocationId, ct),
                DailySales = await GetDailySalesAsync(f, ct),
                TopProducts = await GetTopProductsByValueAsync(10, f, ct),
                //
                TopCustomers = await GetTopCustomersAsync(10, f, ct),
                PaymentCollection = await GetCollectionByPaymentMethodAsync(f, ct),
                SalesByDepartment = await GetSalesByDepartmentAsync(f, ct),
                SalesByBrand = await GetSalesByBrandAsync(10, f, ct),
                SalesByLocation = await GetSalesByLocationAsync(f, ct),
                Inventory = await GetInventorySummaryAsync(f.LocationId, 4, ct),
                TopOutstanding = await GetTopOutstandingInvoicesAsync(10, f, ct),
                RecentInvoices = await GetRecentInvoicesAsync(10, f.LocationId, ct)
            };
        }
    }
}
