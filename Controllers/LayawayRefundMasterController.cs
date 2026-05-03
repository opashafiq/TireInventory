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
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
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
        public async Task<ActionResult<IEnumerable<LayawayRefundMaster>>> GetLayawayRefundMasters()
        {
            return await _context.LayawayRefundMasters.ToListAsync();
        }

        // GET: api/LayawayRefundMaster/5
        [HttpGet("{id}")]
        public async Task<ActionResult<LayawayRefundMaster>> GetLayawayRefundMaster(long id)
        {
            var layawayRefundMaster = await _context.LayawayRefundMasters
                .Include(m => m.tbl_Layaway_Refund_Details)
                .Include(m => m.tbl_Layaway_Refund_Payments)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (layawayRefundMaster == null) return NotFound();

            return layawayRefundMaster;
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