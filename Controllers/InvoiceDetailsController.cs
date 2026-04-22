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
    public class InvoiceDetailsController : ControllerBase
    {
        private readonly ApplicationDBContext _context;

        public InvoiceDetailsController(ApplicationDBContext context)
        {
            _context = context;
        }

        // GET: api/InvoiceDetails
        [HttpGet]
        public async Task<ActionResult<IEnumerable<InvoiceDetails>>> GetInvoiceDetails()
        {
            return await _context.InvoiceDetails.ToListAsync();
        }

        // GET: api/InvoiceDetails/5
        [HttpGet("{id}")]
        public async Task<ActionResult<InvoiceDetails>> GetInvoiceDetails(long id)
        {
            var invoiceDetails = await _context.InvoiceDetails.FindAsync(id);

            if (invoiceDetails == null)
            {
                return NotFound();
            }

            return invoiceDetails;
        }

        // PUT: api/InvoiceDetails/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutInvoiceDetails(long id, InvoiceDetails invoiceDetails)
        {
            if (id != invoiceDetails.Id)
            {
                return BadRequest();
            }

            _context.Entry(invoiceDetails).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!InvoiceDetailsExists(id))
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

        // POST: api/InvoiceDetails
        [HttpPost]
        public async Task<ActionResult<InvoiceDetails>> PostInvoiceDetails(InvoiceDetails invoiceDetails)
        {
            _context.InvoiceDetails.Add(invoiceDetails);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetInvoiceDetails", new { id = invoiceDetails.Id }, invoiceDetails);
        }

        // DELETE: api/InvoiceDetails/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteInvoiceDetails(long id)
        {
            var invoiceDetails = await _context.InvoiceDetails.FindAsync(id);
            if (invoiceDetails == null)
            {
                return NotFound();
            }

            _context.InvoiceDetails.Remove(invoiceDetails);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool InvoiceDetailsExists(long id)
        {
            return _context.InvoiceDetails.Any(e => e.Id == id);
        }
    }
}