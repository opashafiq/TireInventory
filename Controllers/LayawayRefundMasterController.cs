using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TireInventory.Data;
using TireInventory.Models;

namespace TireInventory.Controllers
{
    //[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [Route("api/[controller]")]
    [ApiController]
    public class LayawayRefundMasterController : ControllerBase
    {
        private readonly ApplicationDBContext _context;

        public LayawayRefundMasterController(ApplicationDBContext context)
        {
            _context = context;
        }

        // GET: api/LayawayRefundMaster
        [HttpGet]
        public async Task<ActionResult<IEnumerable<LayawayRefundMasterDto>>> GetLayawayRefundMasters()
        {
            var list = await (from r in _context.LayawayRefundMasters
                              join l in _context.LayawayMasters
                                  on r.Layaway_tbim_InvoiceId equals l.Id into gj
                              from l in gj.DefaultIfEmpty()
                              select new LayawayRefundMasterDto
                              {
                                  Id = r.Id,
                                  tbirm_LayawayRefundDate = r.tbirm_LayawayRefundDate,
                                  tbirm_RefundType = r.tbirm_RefundType,
                                  tbirm_SubTotal = r.tbirm_SubTotal,
                                  tbirm_SaleTax = r.tbirm_SaleTax,
                                  tbirm_Labour = r.tbirm_Labour,
                                  tbirm_DisPer = r.tbirm_DisPer,
                                  tbirm_DisAmt = r.tbirm_DisAmt,
                                  tbirm_Total = r.tbirm_Total,
                                  tbirm_RefundAmt = r.tbirm_RefundAmt,
                                  tbirm_Note = r.tbirm_Note,
                                  UserName = r.UserName,
                                  SetDate = r.SetDate,
                                  OriginalLayawayName = l != null ? l.tbim_Name : string.Empty,
                                  OriginalLayawayDate = l != null ? (DateTime?)l.tbim_InvDate : null
                              }).ToListAsync();

            return Ok(list);
        }

        // GET: api/LayawayRefundMaster/5
        [HttpGet("{id}")]
        public async Task<ActionResult<LayawayRefundMasterDto>> GetLayawayRefundMaster(long id)
        {
            var dto = await (from r in _context.LayawayRefundMasters
                             join l in _context.LayawayMasters
                                 on r.Layaway_tbim_InvoiceId equals l.Id into gj
                             from l in gj.DefaultIfEmpty()
                             where r.Id == id
                             select new LayawayRefundMasterDto
                             {
                                 Id = r.Id,
                                 tbirm_LayawayRefundDate = r.tbirm_LayawayRefundDate,
                                 tbirm_RefundType = r.tbirm_RefundType,
                                 tbirm_SubTotal = r.tbirm_SubTotal,
                                 tbirm_SaleTax = r.tbirm_SaleTax,
                                 tbirm_Labour = r.tbirm_Labour,
                                 tbirm_DisPer = r.tbirm_DisPer,
                                 tbirm_DisAmt = r.tbirm_DisAmt,
                                 tbirm_Total = r.tbirm_Total,
                                 tbirm_RefundAmt = r.tbirm_RefundAmt,
                                 tbirm_Note = r.tbirm_Note,
                                 UserName = r.UserName,
                                 SetDate = r.SetDate,
                                 OriginalLayawayName = l != null ? l.tbim_Name : string.Empty,
                                 OriginalLayawayDate = l != null ? (DateTime?)l.tbim_InvDate : null
                             }).FirstOrDefaultAsync();

            if (dto == null) return NotFound();
            return Ok(dto);
        }

        // NEW: GET: api/LayawayRefundMaster/{id}/details
        // Returns LayawayRefundDetails for a given layaway-refund id (with resolved item names)
        [HttpGet("{id}/details")]
        public async Task<ActionResult<IEnumerable<LayawayRefundDetailsDto>>> GetLayawayRefundDetailsByRefund(long id)
        {
            var list = await _context.LayawayRefundDetails
                .Where(d => d.tbird_Layaway_RefundId == id)
                .Select(d => new LayawayRefundDetailsDto
                {
                    Id = d.Id,
                    tbird_Layaway_RefundId = d.tbird_Layaway_RefundId,
                    tbird_ItemId = d.tbird_ItemId,
                    tbird_ItemCategory = d.tbird_ItemCategory,
                    tbird_DepartmentName = d.tbird_DepartmentName,
                    tbird_Size = d.tbird_Size,
                    tbird_Brand = d.tbird_Brand,
                    tbird_Series = d.tbird_Series,
                    tbird_Bolt = d.tbird_Bolt,
                    tbird_HoleS = d.tbird_HoleS,
                    tbird_Zone = d.tbird_Zone,
                    tbird_DistributorId = d.tbird_DistributorId,
                    tbird_DistributorName = d.tbird_DistributorName,
                    tbird_Qty = d.tbird_Qty,
                    tbird_Layaway_Qty = d.tbird_Layaway_Qty,
                    tbird_Layaway_Qty_LineTotal = d.tbird_Layaway_Qty_LineTotal,
                    tbird_Layaway_Qty_TaxAmt = d.tbird_Layaway_Qty_TaxAmt,
                    tbird_Taxable = d.tbird_Taxable,
                    tbird_UnitPrice = d.tbird_UnitPrice,
                    tbird_LineTotal = d.tbird_LineTotal,
                    tbird_TaxRate = d.tbird_TaxRate,
                    tbird_TaxAmt = d.tbird_TaxAmt,

                    ItemDepartmentName = d.tbird_Item != null && d.tbird_Item.tbim_ItemCategory != null
                        ? d.tbird_Item.tbim_ItemCategory.Tbid_DepartmentName : string.Empty,
                    ItemDistributorName = d.tbird_Item != null && d.tbird_Item.tbim_Distributor != null
                        ? d.tbird_Item.tbim_Distributor.Name : string.Empty,
                    ItemLocationName = d.tbird_Item != null && d.tbird_Item.tbim_Location != null
                        ? d.tbird_Item.tbim_Location.tbld_LocationName : string.Empty,
                    ItemDisplay = d.tbird_Item != null
                        ? (d.tbird_Item.tbim_Brand + " " + d.tbird_Item.tbim_Size).Trim() : string.Empty
                })
                .ToListAsync();

            return Ok(list);
        }

        // NEW: GET: api/LayawayRefundMaster/{id}/payments
        // Returns LayawayRefundPayments for a given layaway-refund id (with resolved refund method name)
        [HttpGet("{id}/payments")]
        public async Task<ActionResult<IEnumerable<LayawayRefundPaymentsDto>>> GetLayawayRefundPaymentsByRefund(long id)
        {
            var list = await (from p in _context.LayawayRefundPayments
                              join rm in _context.RefundMethodNames
                                  on p.tbirp_RefundMethodId equals rm.Id into gj
                              from rm in gj.DefaultIfEmpty()
                              where p.tbirp_Layaway_RefundId == id
                              select new LayawayRefundPaymentsDto
                              {
                                  Id = p.Id,
                                  tbirp_Layaway_RefundId = p.tbirp_Layaway_RefundId,
                                  tbirp_RefundMethodId = p.tbirp_RefundMethodId,
                                  tbirp_RefundAmt = p.tbirp_RefundAmt,
                                  tbirp_Date = p.tbirp_Date,
                                  RefundMethodName = rm != null ? rm.tbrmn_RefundMethodName : string.Empty
                              })
                             .ToListAsync();

            return Ok(list);
        }

        // PUT: api/LayawayRefundMaster/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutLayawayRefundMaster(long id, LayawayRefundMaster layawayRefundMaster)
        {
            if (id != layawayRefundMaster.Id) return BadRequest();

            _context.Entry(layawayRefundMaster).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!LayawayRefundMasterExists(id)) return NotFound();
                throw;
            }

            return NoContent();
        }

        // POST: api/LayawayRefundMaster
        [HttpPost]
        public async Task<ActionResult<LayawayRefundMaster>> PostLayawayRefundMaster(LayawayRefundMaster layawayRefundMaster)
        {
            _context.LayawayRefundMasters.Add(layawayRefundMaster);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetLayawayRefundMaster", new { id = layawayRefundMaster.Id }, layawayRefundMaster);
        }

        // POST: api/LayawayRefundMaster/CreateLayawayRefund
        // Insert master first, then insert refund details and refund payments with FK set to master.Id
        [HttpPost("CreateLayawayRefund")]
        public async Task<ActionResult<LayawayRefundMaster>> CreateLayawayRefund(CreateLayawayRefundDto dto)
        {
            if (dto == null) return BadRequest();

            var refundMaster = dto.Refund ?? new LayawayRefundMaster();

            // Insert master to obtain generated Id
            _context.LayawayRefundMasters.Add(refundMaster);
            await _context.SaveChangesAsync();

            // DETAILS: set FK and enrich from ItemMaster/Departments/Distributors if item provided
            var details = dto.RefundDetails ?? new List<LayawayRefundDetails>();
            foreach (var d in details)
            {
                d.tbird_Layaway_RefundId = refundMaster.Id;
                d.Id = 0;
                d.tbird_Layaway_Refund = null;

                if (d.tbird_ItemId.HasValue)
                {
                    var itemMaster = await _context.ItemMasters.FindAsync(d.tbird_ItemId.Value);
                    if (itemMaster != null)
                    {
                        try
                        {
                            d.tbird_ItemCategory = Convert.ToInt32(itemMaster.tbim_ItemCategoryId);
                        }
                        catch
                        {
                            d.tbird_ItemCategory = null;
                        }

                        var dept = await _context.Departments.FindAsync(itemMaster.tbim_ItemCategoryId);
                        d.tbird_DepartmentName = dept?.Tbid_DepartmentName;

                        d.tbird_Size = itemMaster.tbim_Size;
                        d.tbird_Brand = itemMaster.tbim_Brand;
                        d.tbird_Series = itemMaster.tbim_Series;
                        d.tbird_Bolt = itemMaster.tbim_Bolt;
                        d.tbird_HoleS = itemMaster.tbim_HoleS;
                        d.tbird_Zone = itemMaster.tbim_Zone;
                        d.tbird_DistributorId = itemMaster.tbim_DistributorId;

                        if (itemMaster.tbim_DistributorId.HasValue)
                        {
                            var distributor = await _context.Distributors.FindAsync(itemMaster.tbim_DistributorId.Value);
                            d.tbird_DistributorName = distributor?.Name;
                        }
                    }
                }
            }

            if (details.Count > 0)
            {
                _context.LayawayRefundDetails.AddRange(details);
                await _context.SaveChangesAsync();
            }

            // PAYMENTS: set FK to created master Id and insert
            var payments = dto.RefundPayments ?? new List<LayawayRefundPayments>();
            foreach (var p in payments)
            {
                p.tbirp_Layaway_RefundId = refundMaster.Id;
                p.Id = 0;
                p.tbirp_Layaway_Refund = null;
            }

            if (payments.Count > 0)
            {
                _context.LayawayRefundPayments.AddRange(payments);
                await _context.SaveChangesAsync();
            }

            // Return created master including details and payments
            var created = await _context.LayawayRefundMasters
                .Include(m => m.tbl_Layaway_Refund_Details)
                .Include(m => m.tbl_Layaway_Refund_Payments)
                .FirstOrDefaultAsync(m => m.Id == refundMaster.Id);

            return CreatedAtAction(nameof(GetLayawayRefundMaster), new { id = refundMaster.Id }, created ?? refundMaster);
        }

        // DELETE: api/LayawayRefundMaster/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteLayawayRefundMaster(long id)
        {
            var layawayRefundMaster = await _context.LayawayRefundMasters.FindAsync(id);
            if (layawayRefundMaster == null) return NotFound();

            _context.LayawayRefundMasters.Remove(layawayRefundMaster);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool LayawayRefundMasterExists(long id)
        {
            return _context.LayawayRefundMasters.Any(e => e.Id == id);
        }
    }
}