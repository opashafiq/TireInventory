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
            var invoiceRefundMaster = await _context.InvoiceRefundMasters.FindAsync(id);

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