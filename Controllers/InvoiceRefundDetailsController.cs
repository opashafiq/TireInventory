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
    public class InvoiceRefundDetailsController : ControllerBase
    {
        private readonly ApplicationDBContext _context;

        public InvoiceRefundDetailsController(ApplicationDBContext context)
        {
            _context = context;
        }

        // GET: api/InvoiceRefundDetails
        [HttpGet]
        public async Task<ActionResult<IEnumerable<InvoiceRefundDetails>>> GetInvoiceRefundDetails()
        {
            return await _context.InvoiceRefundDetails.ToListAsync();
        }

        // GET: api/InvoiceRefundDetails/5
        [HttpGet("{id}")]
        public async Task<ActionResult<InvoiceRefundDetails>> GetInvoiceRefundDetails(long id)
        {
            var invoiceRefundDetails = await _context.InvoiceRefundDetails.FindAsync(id);

            if (invoiceRefundDetails == null)
            {
                return NotFound();
            }

            return invoiceRefundDetails;
        }

        // PUT: api/InvoiceRefundDetails/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutInvoiceRefundDetails(long id, InvoiceRefundDetails invoiceRefundDetails)
        {
            if (id != invoiceRefundDetails.Id)
            {
                return BadRequest();
            }

            _context.Entry(invoiceRefundDetails).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!InvoiceRefundDetailsExists(id))
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

        // POST: api/InvoiceRefundDetails
        [HttpPost]
        public async Task<ActionResult<InvoiceRefundDetails>> PostInvoiceRefundDetails(InvoiceRefundDetails invoiceRefundDetails)
        {
            _context.InvoiceRefundDetails.Add(invoiceRefundDetails);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetInvoiceRefundDetails", new { id = invoiceRefundDetails.Id }, invoiceRefundDetails);
        }

        // DELETE: api/InvoiceRefundDetails/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteInvoiceRefundDetails(long id)
        {
            var invoiceRefundDetails = await _context.InvoiceRefundDetails.FindAsync(id);
            if (invoiceRefundDetails == null)
            {
                return NotFound();
            }

            _context.InvoiceRefundDetails.Remove(invoiceRefundDetails);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool InvoiceRefundDetailsExists(long id)
        {
            return _context.InvoiceRefundDetails.Any(e => e.Id == id);
        }
    }
}