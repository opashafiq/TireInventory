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
    public class InvoiceRefundMasterController : ControllerBase
    {
        private readonly ApplicationDBContext _context;

        public InvoiceRefundMasterController(ApplicationDBContext context)
        {
            _context = context;
        }

        // GET: api/InvoiceRefundMaster
        [HttpGet]
        public async Task<ActionResult<IEnumerable<InvoiceRefundMaster>>> GetInvoiceRefundMasters()
        {
            return await _context.InvoiceRefundMasters.ToListAsync();
        }

        // GET: api/InvoiceRefundMaster/5
        [HttpGet("{id}")]
        public async Task<ActionResult<InvoiceRefundMaster>> GetInvoiceRefundMaster(long id)
        {
            var invoiceRefundMaster = await _context.InvoiceRefundMasters
                .Include(m => m.tbl_Invoice_Refund_Details)
                .Include(m => m.tbl_Invoice_Refund_Payments)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (invoiceRefundMaster == null)
            {
                return NotFound();
            }

            return invoiceRefundMaster;
        }

        // PUT: api/InvoiceRefundMaster/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutInvoiceRefundMaster(long id, InvoiceRefundMaster invoiceRefundMaster)
        {
            if (id != invoiceRefundMaster.Id)
            {
                return BadRequest();
            }

            _context.Entry(invoiceRefundMaster).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!InvoiceRefundMasterExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/InvoiceRefundMaster
        [HttpPost]
        public async Task<ActionResult<InvoiceRefundMaster>> PostInvoiceRefundMaster(InvoiceRefundMaster invoiceRefundMaster)
        {
            _context.InvoiceRefundMasters.Add(invoiceRefundMaster);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetInvoiceRefundMaster", new { id = invoiceRefundMaster.Id }, invoiceRefundMaster);
        }

        // POST: api/InvoiceRefundMaster/CreateRefund
        // Mirrors invoice flow: insert refund master first, then insert refund details and refund payments with FK set to refundMaster.Id
        [HttpPost("CreateRefund")]
        public async Task<ActionResult<InvoiceRefundMaster>> CreateRefund(CreateInvoiceRefundDto dto)
        {
            if (dto == null) return BadRequest();

            var refundMaster = dto.Refund ?? new InvoiceRefundMaster();

            // Insert master to obtain generated Id
            _context.InvoiceRefundMasters.Add(refundMaster);
            await _context.SaveChangesAsync();

            // DETAILS: set FK and enrich from ItemMaster/Departments/Distributors as before
            var details = dto.RefundDetails ?? new List<InvoiceRefundDetails>();

            foreach (var d in details)
            {
                d.tbird_InvoiceRefundId = refundMaster.Id;
                d.Id = 0;
                d.tbird_InvoiceRefund = null;

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
                _context.InvoiceRefundDetails.AddRange(details);
                await _context.SaveChangesAsync();
            }

            // PAYMENTS: set FK to created refund master Id and insert
            var payments = dto.RefundPayments ?? new List<InvoiceRefundPayments>();
            foreach (var p in payments)
            {
                p.tbirp_InvoiceRefundId = refundMaster.Id;
                p.Id = 0;
                p.tbirp_InvoiceRefund = null;
            }

            if (payments.Count > 0)
            {
                _context.InvoiceRefundPayments.AddRange(payments);
                await _context.SaveChangesAsync();
            }

            // Return created refund master including its details and payments
            var created = await _context.InvoiceRefundMasters
                .Include(m => m.tbl_Invoice_Refund_Details)
                .Include(m => m.tbl_Invoice_Refund_Payments)
                .FirstOrDefaultAsync(m => m.Id == refundMaster.Id);

            return CreatedAtAction(nameof(GetInvoiceRefundMaster), new { id = refundMaster.Id }, created ?? refundMaster);
        }

        // DELETE: api/InvoiceRefundMaster/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteInvoiceRefundMaster(long id)
        {
            var invoiceRefundMaster = await _context.InvoiceRefundMasters.FindAsync(id);
            if (invoiceRefundMaster == null)
            {
                return NotFound();
            }

            _context.InvoiceRefundMasters.Remove(invoiceRefundMaster);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool InvoiceRefundMasterExists(long id)
        {
            return _context.InvoiceRefundMasters.Any(e => e.Id == id);
        }
    }
}