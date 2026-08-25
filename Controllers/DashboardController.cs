using Microsoft.AspNetCore.Mvc;
using TireInventory.Models;
using TireInventory.Services;

namespace TireInventory.Controllers
{
    /// <summary>
    /// Every endpoint accepts the same period contract:
    ///   ?period=today|yesterday|wtd|mtd|lastmonth|ytd|lastyear|last12m|custom
    ///   &amp;from=2026-01-01&amp;to=2026-01-31   (only when period=custom)
    ///   &amp;locationId=3                      (optional branch filter)
    /// Default period is MTD.
    /// </summary>
    
    
    //[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [ApiController]
    [Route("api/dashboard")]
    // [Authorize]   // <- enable once wired into your auth pipeline
    public class DashboardController : ControllerBase
    {
        private readonly IDashboardService _dashboard;

        public DashboardController(IDashboardService dashboard)
        {
            _dashboard = dashboard;
        }

        private static DashboardFilter F(string? period, DateTime? from, DateTime? to, long? locationId)
            => DashboardFilter.Resolve(period, from, to, locationId);

        // ---------- one-shot payload for the initial page load ----------

        [HttpGet("overview")]
        public async Task<IActionResult> Overview(string? period, DateTime? from, DateTime? to, long? locationId, CancellationToken ct)
            => Ok(await _dashboard.GetOverviewAsync(F(period, from, to, locationId), ct));

        // ---------- KPI cards ----------

        [HttpGet("kpi")]
        public async Task<IActionResult> Kpi(string? period, DateTime? from, DateTime? to, long? locationId, CancellationToken ct)
            => Ok(await _dashboard.GetKpiSummaryAsync(F(period, from, to, locationId), ct));

        // ---------- trends ----------

        [HttpGet("sales/yearly")]
        public async Task<IActionResult> YearlySales(int years = 5, long? locationId = null, CancellationToken ct = default)
            => Ok(await _dashboard.GetYearlySalesAsync(years, locationId, ct));

        [HttpGet("sales/monthly")]
        public async Task<IActionResult> MonthlySales(int months = 12, long? locationId = null, CancellationToken ct = default)
            => Ok(await _dashboard.GetMonthlySalesAsync(months, locationId, ct));

        [HttpGet("sales/daily")]
        public async Task<IActionResult> DailySales(string? period, DateTime? from, DateTime? to, long? locationId, CancellationToken ct)
            => Ok(await _dashboard.GetDailySalesAsync(F(period, from, to, locationId), ct));

        [HttpGet("sales/heatmap")]
        public async Task<IActionResult> Heatmap(string? period, DateTime? from, DateTime? to, long? locationId, CancellationToken ct)
            => Ok(await _dashboard.GetSalesHeatmapAsync(F(period, from, to, locationId), ct));

        // ---------- money in ----------

        [HttpGet("collection/by-payment-method")]
        public async Task<IActionResult> CollectionByPaymentMethod(string? period, DateTime? from, DateTime? to, long? locationId, CancellationToken ct)
            => Ok(await _dashboard.GetCollectionByPaymentMethodAsync(F(period, from, to, locationId), ct));

        [HttpGet("layaway/summary")]
        public async Task<IActionResult> Layaway(string? period, DateTime? from, DateTime? to, long? locationId, CancellationToken ct)
            => Ok(await _dashboard.GetLayawaySummaryAsync(F(period, from, to, locationId), ct));

        // ---------- product mix ----------

        [HttpGet("products/top-by-value")]
        public async Task<IActionResult> TopProductsByValue(int top = 10, string? period = null, DateTime? from = null, DateTime? to = null, long? locationId = null, CancellationToken ct = default)
            => Ok(await _dashboard.GetTopProductsByValueAsync(top, F(period, from, to, locationId), ct));

        [HttpGet("products/top-by-quantity")]
        public async Task<IActionResult> TopProductsByQuantity(int top = 10, string? period = null, DateTime? from = null, DateTime? to = null, long? locationId = null, CancellationToken ct = default)
            => Ok(await _dashboard.GetTopProductsByQuantityAsync(top, F(period, from, to, locationId), ct));

        [HttpGet("sales/by-department")]
        public async Task<IActionResult> SalesByDepartment(string? period, DateTime? from, DateTime? to, long? locationId, CancellationToken ct)
            => Ok(await _dashboard.GetSalesByDepartmentAsync(F(period, from, to, locationId), ct));

        [HttpGet("sales/by-brand")]
        public async Task<IActionResult> SalesByBrand(int top = 10, string? period = null, DateTime? from = null, DateTime? to = null, long? locationId = null, CancellationToken ct = default)
            => Ok(await _dashboard.GetSalesByBrandAsync(top, F(period, from, to, locationId), ct));

        [HttpGet("sales/by-distributor")]
        public async Task<IActionResult> SalesByDistributor(int top = 10, string? period = null, DateTime? from = null, DateTime? to = null, long? locationId = null, CancellationToken ct = default)
            => Ok(await _dashboard.GetSalesByDistributorAsync(top, F(period, from, to, locationId), ct));

        // ---------- customers ----------

        [HttpGet("customers/top")]
        public async Task<IActionResult> TopCustomers(int top = 10, string? period = null, DateTime? from = null, DateTime? to = null, long? locationId = null, CancellationToken ct = default)
            => Ok(await _dashboard.GetTopCustomersAsync(top, F(period, from, to, locationId), ct));

        [HttpGet("customers/mix")]
        public async Task<IActionResult> CustomerMix(string? period, DateTime? from, DateTime? to, long? locationId, CancellationToken ct)
            => Ok(await _dashboard.GetCustomerMixAsync(F(period, from, to, locationId), ct));

        [HttpGet("vehicles/top-makes")]
        public async Task<IActionResult> TopVehicleMakes(int top = 10, string? period = null, DateTime? from = null, DateTime? to = null, long? locationId = null, CancellationToken ct = default)
            => Ok(await _dashboard.GetTopVehicleMakesAsync(top, F(period, from, to, locationId), ct));

        [HttpGet("vehicles/tire-positions")]
        public async Task<IActionResult> TirePositions(string? period, DateTime? from, DateTime? to, long? locationId, CancellationToken ct)
            => Ok(await _dashboard.GetTirePositionDemandAsync(F(period, from, to, locationId), ct));

        // ---------- branch & risk ----------

        [HttpGet("sales/by-location")]
        public async Task<IActionResult> SalesByLocation(string? period, DateTime? from, DateTime? to, CancellationToken ct)
            => Ok(await _dashboard.GetSalesByLocationAsync(F(period, from, to, null), ct));

        [HttpGet("invoices/top-outstanding")]
        public async Task<IActionResult> TopOutstanding(int top = 10, string? period = null, DateTime? from = null, DateTime? to = null, long? locationId = null, CancellationToken ct = default)
            => Ok(await _dashboard.GetTopOutstandingInvoicesAsync(top, F(period, from, to, locationId), ct));

        [HttpGet("invoices/recent")]
        public async Task<IActionResult> RecentInvoices(int top = 10, long? locationId = null, CancellationToken ct = default)
            => Ok(await _dashboard.GetRecentInvoicesAsync(top, locationId, ct));

        // ---------- inventory ----------

        [HttpGet("inventory/summary")]
        public async Task<IActionResult> InventorySummary(long? locationId = null, int lowStockThreshold = 4, CancellationToken ct = default)
            => Ok(await _dashboard.GetInventorySummaryAsync(locationId, lowStockThreshold, ct));

        [HttpGet("inventory/low-stock")]
        public async Task<IActionResult> LowStock(int threshold = 4, int top = 20, long? locationId = null, CancellationToken ct = default)
            => Ok(await _dashboard.GetLowStockItemsAsync(threshold, top, locationId, ct));

        [HttpGet("inventory/dead-stock")]
        public async Task<IActionResult> DeadStock(int days = 180, int top = 20, long? locationId = null, CancellationToken ct = default)
            => Ok(await _dashboard.GetDeadStockAsync(days, top, locationId, ct));
    }
}
