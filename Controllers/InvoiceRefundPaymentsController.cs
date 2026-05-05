using System.Linq;
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
    public class InvoiceRefundPaymentsController : ControllerBase
    {
        private readonly ApplicationDBContext _context;

        public InvoiceRefundPaymentsController(ApplicationDBContext context)
        {
            _context = context;
        }

        // GET: api/InvoiceRefundPayments
        [HttpGet]
        public async Task<ActionResult<IEnumerable<InvoiceRefundPaymentsDto>>> GetInvoiceRefundPayments()
        {
            var list = await (from p in _context.InvoiceRefundPayments
                              join rm in _context.RefundMethodNames
                                  on p.tbirp_RefundMethodId equals rm.Id into gj
                              from rm in gj.DefaultIfEmpty()
                              select new InvoiceRefundPaymentsDto
                              {
                                  Id = p.Id,
                                  tbirp_InvoiceRefundId = p.tbirp_InvoiceRefundId,
                                  tbirp_RefundMethodId = p.tbirp_RefundMethodId,
                                  tbirp_RefundAmt = p.tbirp_RefundAmt,
                                  tbirp_Date = p.tbirp_Date,
                                  RefundMethodName = rm != null ? rm.tbrmn_RefundMethodName : string.Empty
                              })
                             .ToListAsync();

            return Ok(list);
        }

        // GET: api/InvoiceRefundPayments/5
        [HttpGet("{id}")]
        public async Task<ActionResult<InvoiceRefundPaymentsDto>> GetInvoiceRefundPayments(long id)
        {
            var dto = await (from p in _context.InvoiceRefundPayments
                             join rm in _context.RefundMethodNames
                                 on p.tbirp_RefundMethodId equals rm.Id into gj
                             from rm in gj.DefaultIfEmpty()
                             where p.Id == id
                             select new InvoiceRefundPaymentsDto
                             {
                                 Id = p.Id,
                                 tbirp_InvoiceRefundId = p.tbirp_InvoiceRefundId,
                                 tbirp_RefundMethodId = p.tbirp_RefundMethodId,
                                 tbirp_RefundAmt = p.tbirp_RefundAmt,
                                 tbirp_Date = p.tbirp_Date,
                                 RefundMethodName = rm != null ? rm.tbrmn_RefundMethodName : string.Empty
                             })
                            .FirstOrDefaultAsync();

            if (dto == null) return NotFound();
            return Ok(dto);
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