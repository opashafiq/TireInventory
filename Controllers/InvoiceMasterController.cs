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
    public class InvoiceMasterController : ControllerBase
    {
        private readonly ApplicationDBContext _context;

        public InvoiceMasterController(ApplicationDBContext context)
        {
            _context = context;
        }

        // GET: api/InvoiceMaster
        [HttpGet]
        public async Task<ActionResult<IEnumerable<InvoiceMaster>>> GetInvoiceMasters()
        {
            return await _context.InvoiceMasters.ToListAsync();
        }

        // GET: api/InvoiceMaster/5
        [HttpGet("{id}")]
        public async Task<ActionResult<InvoiceMaster>> GetInvoiceMaster(long id)
        {
            var invoiceMaster = await _context.InvoiceMasters.FindAsync(id);

            if (invoiceMaster == null)
            {
                return NotFound();
            }

            return invoiceMaster;
        }

        // PUT: api/InvoiceMaster/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutInvoiceMaster(long id, InvoiceMaster invoiceMaster)
        {
            if (id != invoiceMaster.Id)
            {
                return BadRequest();
            }

            _context.Entry(invoiceMaster).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!InvoiceMasterExists(id))
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

        // POST: api/InvoiceMaster
        [HttpPost]
        public async Task<ActionResult<InvoiceMaster>> PostInvoiceMaster(InvoiceMaster invoiceMaster)
        {
            _context.InvoiceMasters.Add(invoiceMaster);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetInvoiceMaster", new { id = invoiceMaster.Id }, invoiceMaster);
        }

        // POST: api/InvoiceMaster/CreateInvoice
        // Accepts a master, a list of details and a list of payments.
        // Inserts master first, then inserts details and payments with FK set to master.Id
        [HttpPost("CreateInvoice")]
        public async Task<ActionResult<InvoiceMaster>> CreateInvoice(CreateInvoiceDto dto)
        {
            if (dto == null) return BadRequest();

            var invoiceMaster = dto.Invoice ?? new InvoiceMaster();

            // Add master first to obtain generated Id
            _context.InvoiceMasters.Add(invoiceMaster);
            await _context.SaveChangesAsync();

            // DETAILS: set FK and enrich from ItemMaster/Departments/Distributors as before
            var details = dto.InvoiceDetails ?? new List<InvoiceDetails>();
            foreach (var d in details)
            {
                d.tbid_InvoiceId = invoiceMaster.Id;
                d.Id = 0;
                d.tbid_Invoice = null;

                if (d.tbid_ItemId.HasValue)
                {
                    var itemMaster = await _context.ItemMasters.FindAsync(d.tbid_ItemId.Value);
                    if (itemMaster != null)
                    {
                        try
                        {
                            d.tbid_ItemCategory = (long?)itemMaster.tbim_ItemCategoryId;
                        }
                        catch
                        {
                            d.tbid_ItemCategory = null;
                        }

                        var dept = await _context.Departments.FindAsync(itemMaster.tbim_ItemCategoryId);
                        d.tbid_DepartmentName = dept?.Tbid_DepartmentName;

                        d.tbid_Size = itemMaster.tbim_Size;
                        d.tbid_Brand = itemMaster.tbim_Brand;
                        d.tbid_Series = itemMaster.tbim_Series;
                        d.tbid_Bolt = itemMaster.tbim_Bolt;
                        d.tbid_HoleS = itemMaster.tbim_HoleS;
                        d.tbid_Zone = itemMaster.tbim_Zone;
                        d.tbid_DistributorId = itemMaster.tbim_DistributorId;

                        if (itemMaster.tbim_DistributorId.HasValue)
                        {
                            var distributor = await _context.Distributors.FindAsync(itemMaster.tbim_DistributorId.Value);
                            d.tbid_DistributorName = distributor?.Name;
                        }
                    }
                }
            }

            if (details.Count > 0)
            {
                _context.InvoiceDetails.AddRange(details);
                await _context.SaveChangesAsync();
            }

            // PAYMENTS: set FK to created master Id and insert
            var payments = dto.InvoicePayments ?? new List<InvoicePayments>();
            foreach (var p in payments)
            {
                p.tbip_InvoiceId = invoiceMaster.Id;
                p.Id = 0;
                p.tbip_Invoice = null;
            }

            if (payments.Count > 0)
            {
                _context.InvoicePayments.AddRange(payments);
                await _context.SaveChangesAsync();
            }

            // Optionally load created invoice with details and payments to return
            var created = await _context.InvoiceMasters
                .Include(m => m.tbl_Invoice_Details)
                .Include(m => m.tbl_Invoice_Payments)
                .FirstOrDefaultAsync(m => m.Id == invoiceMaster.Id);

            return CreatedAtAction("GetInvoiceMaster", new { id = invoiceMaster.Id }, created ?? invoiceMaster);
        }

        // DELETE: api/InvoiceMaster/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteInvoiceMaster(long id)
        {
            var invoiceMaster = await _context.InvoiceMasters.FindAsync(id);
            if (invoiceMaster == null)
            {
                return NotFound();
            }

            _context.InvoiceMasters.Remove(invoiceMaster);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool InvoiceMasterExists(long id)
        {
            return _context.InvoiceMasters.Any(e => e.Id == id);
        }
    }
}