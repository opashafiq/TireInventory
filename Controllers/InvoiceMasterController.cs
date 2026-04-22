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