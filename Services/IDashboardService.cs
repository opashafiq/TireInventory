using TireInventory.Models;

namespace TireInventory.Services
{
    public interface IDashboardService
    {
        // KPI
        Task<KpiSummaryDto> GetKpiSummaryAsync(DashboardFilter filter, CancellationToken ct = default);

        // Trends
        Task<List<YearlySalesDto>> GetYearlySalesAsync(int years, long? locationId, CancellationToken ct = default);
        Task<List<MonthlySalesDto>> GetMonthlySalesAsync(int months, long? locationId, CancellationToken ct = default);
        Task<List<DailySalesDto>> GetDailySalesAsync(DashboardFilter filter, CancellationToken ct = default);
        Task<List<HeatmapCellDto>> GetSalesHeatmapAsync(DashboardFilter filter, CancellationToken ct = default);

        // Money in
        Task<List<PaymentCollectionDto>> GetCollectionByPaymentMethodAsync(DashboardFilter filter, CancellationToken ct = default);
        Task<LayawaySummaryDto> GetLayawaySummaryAsync(DashboardFilter filter, CancellationToken ct = default);

        // Product mix
        Task<List<TopProductDto>> GetTopProductsByValueAsync(int top, DashboardFilter filter, CancellationToken ct = default);
        Task<List<TopProductDto>> GetTopProductsByQuantityAsync(int top, DashboardFilter filter, CancellationToken ct = default);
        Task<List<NameValueDto>> GetSalesByDepartmentAsync(DashboardFilter filter, CancellationToken ct = default);
        Task<List<NameValueDto>> GetSalesByBrandAsync(int top, DashboardFilter filter, CancellationToken ct = default);
        Task<List<NameValueDto>> GetSalesByDistributorAsync(int top, DashboardFilter filter, CancellationToken ct = default);

        // Customers
        Task<List<TopCustomerDto>> GetTopCustomersAsync(int top, DashboardFilter filter, CancellationToken ct = default);
        Task<CustomerMixDto> GetCustomerMixAsync(DashboardFilter filter, CancellationToken ct = default);
        Task<List<NameValueDto>> GetTopVehicleMakesAsync(int top, DashboardFilter filter, CancellationToken ct = default);
        Task<TirePositionDto> GetTirePositionDemandAsync(DashboardFilter filter, CancellationToken ct = default);

        // Branch / risk
        Task<List<NameValueDto>> GetSalesByLocationAsync(DashboardFilter filter, CancellationToken ct = default);
        Task<List<OutstandingInvoiceDto>> GetTopOutstandingInvoicesAsync(int top, DashboardFilter filter, CancellationToken ct = default);
        Task<List<RecentInvoiceDto>> GetRecentInvoicesAsync(int top, long? locationId, CancellationToken ct = default);

        // Inventory
        Task<InventorySummaryDto> GetInventorySummaryAsync(long? locationId, int lowStockThreshold = 4, CancellationToken ct = default);
        Task<List<StockItemDto>> GetLowStockItemsAsync(int threshold, int top, long? locationId, CancellationToken ct = default);
        Task<List<StockItemDto>> GetDeadStockAsync(int days, int top, long? locationId, CancellationToken ct = default);

        // Everything for the first paint
        Task<DashboardOverviewDto> GetOverviewAsync(DashboardFilter filter, CancellationToken ct = default);
    }
}
