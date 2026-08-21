using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TireInventory.Data;
using TireInventory.Models;
using TireInventory.Models.ReportDtos;
using TireInventory.Helpers;

namespace TireInventory.Controllers
{
    //[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [Route("api/[controller]")]
    [ApiController]
    public class ReportsController : ControllerBase
    {
        private readonly ApplicationDBContext _context;

        public ReportsController(ApplicationDBContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Returns ItemMaster rows for the specified CategoryId joined with Departments as OURPTotalDto.
        /// CategoryId matches ItemMaster.tbim_ItemCategoryId (department Id).
        /// Total = tbim_Qty * tbim_OURP (uses 0 when tbim_OURP is null).
        /// </summary>
        [HttpGet("GetTotalOURPByCategory/{categoryId:long}")]
        public async Task<ActionResult<List<OURPTotalDto>>> GetTotalOURPByCategory(long categoryId)
        {
            var query = _context.ItemMasters
                .Include(im => im.tbim_ItemCategory)
                .Where(im => im.tbim_ItemCategoryId == categoryId)
                .Select(im => new OURPTotalDto
                {
                    Category = im.tbim_ItemCategory != null ? im.tbim_ItemCategory.Tbid_DepartmentName : "Unassigned",
                    Size = im.tbim_Size ?? string.Empty,
                    Brand = im.tbim_Brand ?? string.Empty,
                    Series = im.tbim_Series ?? string.Empty,
                    Bolt = im.tbim_Bolt ?? string.Empty,
                    HoleS = im.tbim_HoleS ?? string.Empty,
                    Zone = im.tbim_Zone ?? string.Empty,
                    Qty = im.tbim_Qty,
                    OURP = im.tbim_OURP ?? 0m,
                    Total = (im.tbim_OURP ?? 0m) * im.tbim_Qty
                });

            var result = await query.ToListAsync();
            return Ok(result);
        }

        /// <summary>
        /// Returns total OURP per category (Total = sum(tbim_Qty * tbim_OURP)), grouped by tbim_ItemCategoryId.
        /// Category is Departments.Tbid_DepartmentName.
        /// </summary>
        [HttpGet("GetTotalOURP")]
        public async Task<ActionResult<List<TotalOURPDto>>> GetTotalOURP()
        {
            var grouped = _context.ItemMasters
                .AsNoTracking()
                .GroupBy(im => im.tbim_ItemCategoryId)
                .Select(g => new
                {
                    CategoryId = g.Key,
                    Total = g.Sum(i => (i.tbim_OURP ?? 0m) * i.tbim_Qty)
                });

            var result = await grouped
                .Join(
                    _context.Departments,
                    g => g.CategoryId,
                    d => d.Id,
                    (g, d) => new TotalOURPDto
                    {
                        Category = d.Tbid_DepartmentName,
                        Total = g.Total
                    })
                .ToListAsync();

            return Ok(result);
        }

        /// <summary>
        /// Returns distinct customers from InvoiceMaster and LayawayMaster.
        /// Distinct is based on LTRIM(Name), EmailAddress and Phone.
        /// Filters by tbim_InvDate using optional startDate and endDate query parameters.
        /// </summary>
        [HttpGet("GetCustomerList")]
        public async Task<ActionResult<List<CustomersDto>>> GetCustomerList([FromQuery] DateTime? startDate = null, [FromQuery] DateTime? endDate = null)
        {
            var invoicesQuery = _context.InvoiceMasters
                .AsNoTracking()
                .Where(im => (!startDate.HasValue || im.tbim_InvDate >= startDate.Value) &&
                             (!endDate.HasValue || im.tbim_InvDate <= endDate.Value))
                .Select(im => new { Name = im.tbim_Name, Email = im.tbim_EmailAddress, Phone = im.tbim_Phone });

            var layawaysQuery = _context.LayawayMasters
                .AsNoTracking()
                .Where(l => (!startDate.HasValue || l.tbim_InvDate >= startDate.Value) &&
                            (!endDate.HasValue || l.tbim_InvDate <= endDate.Value))
                .Select(l => new { Name = l.tbim_Name, Email = l.tbim_EmailAddress, Phone = l.tbim_Phone });

            var unionList = await invoicesQuery
                .Union(layawaysQuery)
                .ToListAsync();

            var customers = unionList
                .Select(x => new CustomersDto
                {
                    Name = (x.Name ?? string.Empty).TrimStart(),
                    EmailAddress = x.Email ?? string.Empty,
                    Phone = x.Phone ?? string.Empty
                })
                .GroupBy(c => new { c.Name, c.EmailAddress, c.Phone })
                .Select(g => g.First())
                .ToList();

            return Ok(customers);
        }

        /// <summary>
        /// New tire sales report.
        /// Inner join InvoiceMaster and InvoiceDetails.
        /// Filters by optional date range (InvoiceMaster.tbim_InvDate), Brand and Size.
        /// Only InvoiceDetails with tbid_DepartmentName == "New Tires" are included.
        /// Groups by department, size and brand and returns summed quantity.
        /// </summary>
        [HttpGet("GetTireSellReport")]
        public async Task<ActionResult<List<NewTireSaleDto>>> GetTireSellReport(
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null,
            [FromQuery] string Brand = null,
            [FromQuery] string Size = null,
            [FromQuery] string Export = null)
        {
            var query = from det in _context.InvoiceDetails.AsNoTracking()
                        join inv in _context.InvoiceMasters.AsNoTracking() on det.tbid_InvoiceId equals inv.Id
                        where det.tbid_DepartmentName == "New Tires"
                              && (!startDate.HasValue || inv.tbim_InvDate >= startDate.Value)
                              && (!endDate.HasValue || inv.tbim_InvDate <= endDate.Value)
                              && (string.IsNullOrEmpty(Brand) || det.tbid_Brand == Brand)
                              && (string.IsNullOrEmpty(Size) || det.tbid_Size == Size)
                        group det by new
                        {
                            Category = det.tbid_DepartmentName ?? "Unassigned",
                            Brand = det.tbid_Brand ?? string.Empty,
                            Size = det.tbid_Size ?? string.Empty
                        } into g
                        select new NewTireSaleDto
                        {
                            Category = g.Key.Category,
                            Brand = g.Key.Brand,
                            Size = g.Key.Size,
                            Qty = g.Sum(x => x.tbid_Qty)
                        };

            var result = await query.ToListAsync();

            // Export parameter reserved for future export behavior; currently ignored.
            return Ok(result);
        }

        /// <summary>
        /// New wheels sales report.
        /// Inner join InvoiceMaster and InvoiceDetails.
        /// Filters by optional date range (InvoiceMaster.tbim_InvDate), Brand, Size, Bolt and Series.
        /// Only InvoiceDetails with tbid_DepartmentName == "New Wheels" are included.
        /// Groups by department, size, brand, series and bolt and returns summed quantity.
        /// </summary>
        [HttpGet("GetWheelSaleReport")]
        public async Task<ActionResult<List<NewWheelsSale>>> GetWheelSaleReport(
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null,
            [FromQuery] string Brand = null,
            [FromQuery] string Size = null,
            [FromQuery] string Bolt = null,
            [FromQuery] string Series = null)
        {
            var query = from det in _context.InvoiceDetails.AsNoTracking()
                        join inv in _context.InvoiceMasters.AsNoTracking() on det.tbid_InvoiceId equals inv.Id
                        where det.tbid_DepartmentName == "New Wheels"
                              && (!startDate.HasValue || inv.tbim_InvDate >= startDate.Value)
                              && (!endDate.HasValue || inv.tbim_InvDate <= endDate.Value)
                              && (string.IsNullOrEmpty(Brand) || det.tbid_Brand == Brand)
                              && (string.IsNullOrEmpty(Size) || det.tbid_Size == Size)
                              && (string.IsNullOrEmpty(Bolt) || det.tbid_Bolt == Bolt)
                              && (string.IsNullOrEmpty(Series) || det.tbid_Series == Series)
                        group det by new
                        {
                            Category = det.tbid_DepartmentName ?? "Unassigned",
                            Size = det.tbid_Size ?? string.Empty,
                            Brand = det.tbid_Brand ?? string.Empty,
                            Series = det.tbid_Series ?? string.Empty,
                            Bolt = det.tbid_Bolt ?? string.Empty
                        } into g
                        select new NewWheelsSale
                        {
                            Category = g.Key.Category,
                            Size = g.Key.Size,
                            Brand = g.Key.Brand,
                            Series = g.Key.Series,
                            Bolt = g.Key.Bolt,
                            Qty = g.Sum(x => x.tbid_Qty)
                        };

            var result = await query.ToListAsync();
            return Ok(result);
        }

        /// <summary>
        /// Sales summary report.
        /// Mirrors stored procedure logic:
        /// - if Category is null: group InvoiceDetails by department and size over date range
        /// - if Category provided: same but filtered by SD.tbid_ItemCategory = Category
        /// - compute AvgPrice as AVG(tbid_UnitPrice) and TotalQty as SUM(tbid_Qty)
        /// - when Category provided, retrieve MIN(tbim_OURP) from ItemMaster per size and left-join
        /// </summary>
        [HttpGet("GetSaleSummary")]
        public async Task<ActionResult<List<SalesSummaryDto>>> GetSaleSummary(
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null,
            [FromQuery] long? Category = null)
        {
            // Base grouped query following the stored procedure grouping
            var baseQuery = from sd in _context.InvoiceDetails.AsNoTracking()
                            join sm in _context.InvoiceMasters.AsNoTracking() on sd.tbid_InvoiceId equals sm.Id
                            where (!startDate.HasValue || sm.tbim_InvDate >= startDate.Value)
                                  && (!endDate.HasValue || sm.tbim_InvDate <= endDate.Value)
                                  && (!Category.HasValue || sd.tbid_ItemCategory == Category.Value)
                            group sd by new
                            {
                                CategoryName = sd.tbid_DepartmentName ?? string.Empty,
                                Size = sd.tbid_Size ?? string.Empty
                            } into g
                            select new
                            {
                                CategoryName = g.Key.CategoryName,
                                Brand = (string)null,
                                Size = g.Key.Size,
                                TotalQty = g.Sum(x => x.tbid_Qty),
                                AvgPrice = g.Average(x => x.tbid_UnitPrice)
                            };

            var saleSummary = await baseQuery
                .OrderByDescending(x => x.TotalQty)
                .ToListAsync();

            // Build OURP lookup only when Category is provided (matches stored procedure behavior)
            Dictionary<string, decimal> ourpBySize = new Dictionary<string, decimal>(StringComparer.OrdinalIgnoreCase);
            // only populate when Category provided
            if (Category.HasValue)
            {
                var ourpList = await _context.ItemMasters
                    .AsNoTracking()
                    .Where(im => im.tbim_ItemCategoryId == Category.Value)
                    .GroupBy(im => im.tbim_Size)
                    .Select(g => new { Size = g.Key ?? string.Empty, OURP = g.Min(i => i.tbim_OURP) })
                    .ToListAsync();

                foreach (var o in ourpList)
                    ourpBySize[o.Size ?? string.Empty] = o.OURP ?? 0m;
            }

            // Map to DTO and left-join OURP by Size (if present), default 0 when missing
            var result = saleSummary.Select(s => new SalesSummaryDto
            {
                CategoryName = s.CategoryName,
                Brand = null,
                Size = s.Size,
                TotalQty = s.TotalQty,
                AvgPrice = Math.Round(s.AvgPrice, 2),
                tbim_OURP = ourpBySize.TryGetValue(s.Size ?? string.Empty, out var v) ? v : 0m
            }).ToList();

            return Ok(result);
        }
    }
}
