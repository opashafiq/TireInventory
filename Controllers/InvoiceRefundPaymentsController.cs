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
    public class InvoiceRefundPaymentsController : ControllerBase
    {
        private readonly ApplicationDBContext _context;

        public InvoiceRefundPaymentsController(ApplicationDBContext context)
        {
            _context = context;
        }

        // GET: api/InvoiceRefundPayments
        [HttpGet]
        public async Task<ActionResult<IEnumerable<InvoiceRefundPayments>>> GetInvoiceRefundPayments()
        {
            return await _context.InvoiceRefundPayments.ToListAsync();
        }

        // GET: api/InvoiceRefundPayments/5
        [HttpGet("{id}")]
        public async Task<ActionResult<InvoiceRefundPayments>> GetInvoiceRefundPayments(long id)
        {
            var invoiceRefundPayments = await _context.InvoiceRefundPayments.FindAsync(id);

            if (invoiceRefundPayments == null)
            {
                return NotFound();
            }

            return invoiceRefundPayments;
        }

        // PUT: api/InvoiceRefundPayments/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutInvoiceRefundPayments(long id, InvoiceRefundPayments invoiceRefundPayments)
        {
            if (id != invoiceRefundPayments.Id)
            {
                return BadRequest();
            }

            _context.Entry(invoiceRefundPayments).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!InvoiceRefundPaymentsExists(id))
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

        // POST: api/InvoiceRefundPayments
        [HttpPost]
        public async Task<ActionResult<InvoiceRefundPayments>> PostInvoiceRefundPayments(InvoiceRefundPayments invoiceRefundPayments)
        {
            _context.InvoiceRefundPayments.Add(invoiceRefundPayments);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetInvoiceRefundPayments", new { id = invoiceRefundPayments.Id }, invoiceRefundPayments);
        }

        // DELETE: api/InvoiceRefundPayments/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteInvoiceRefundPayments(long id)
        {
            var invoiceRefundPayments = await _context.InvoiceRefundPayments.FindAsync(id);
            if (invoiceRefundPayments == null)
            {
                return NotFound();
            }

            _context.InvoiceRefundPayments.Remove(invoiceRefundPayments);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool InvoiceRefundPaymentsExists(long id)
        {
            return _context.InvoiceRefundPayments.Any(e => e.Id == id);
        }
    }
}