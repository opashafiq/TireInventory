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
    public class InvoicePaymentsController : ControllerBase
    {
        private readonly ApplicationDBContext _context;

        public InvoicePaymentsController(ApplicationDBContext context)
        {
            _context = context;
        }

        // GET: api/InvoicePayments
        [HttpGet]
        public async Task<ActionResult<IEnumerable<InvoicePaymentsDto>>> GetInvoicePayments()
        {
            var list = await (from p in _context.InvoicePayments
                              join pay in _context.PaymentNames
                                  on p.tbip_PaymentId equals pay.Id into gj
                              from pay in gj.DefaultIfEmpty()
                              select new InvoicePaymentsDto
                              {
                                  Id = p.Id,
                                  tbip_InvoiceId = p.tbip_InvoiceId,
                                  tbip_PaymentId = p.tbip_PaymentId,
                                  tbip_PayAmt = p.tbip_PayAmt,
                                  tbip_Date = p.tbip_Date,
                                  tbip_PaymentTypeId = p.tbip_PaymentTypeId,
                                  PaymentName = pay != null ? pay.tbpn_PaymentName : string.Empty
                              })
                             .ToListAsync();

            return Ok(list);
        }

        // GET: api/InvoicePayments/5
        [HttpGet("{id}")]
        public async Task<ActionResult<InvoicePaymentsDto>> GetInvoicePayments(long id)
        {
            var dto = await (from p in _context.InvoicePayments
                             join pay in _context.PaymentNames
                                 on p.tbip_PaymentId equals pay.Id into gj
                             from pay in gj.DefaultIfEmpty()
                             where p.Id == id
                             select new InvoicePaymentsDto
                             {
                                 Id = p.Id,
                                 tbip_InvoiceId = p.tbip_InvoiceId,
                                 tbip_PaymentId = p.tbip_PaymentId,
                                 tbip_PayAmt = p.tbip_PayAmt,
                                 tbip_Date = p.tbip_Date,
                                 tbip_PaymentTypeId = p.tbip_PaymentTypeId,
                                 PaymentName = pay != null ? pay.tbpn_PaymentName : string.Empty
                             })
                            .FirstOrDefaultAsync();

            if (dto == null) return NotFound();
            return Ok(dto);
        }

        // PUT: api/InvoicePayments/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutInvoicePayments(long id, InvoicePayments invoicePayments)
        {
            if (id != invoicePayments.Id)
            {
                return BadRequest();
            }

            _context.Entry(invoicePayments).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!InvoicePaymentsExists(id))
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

        // POST: api/InvoicePayments
        [HttpPost]
        public async Task<ActionResult<InvoicePayments>> PostInvoicePayments(InvoicePayments invoicePayments)
        {
            _context.InvoicePayments.Add(invoicePayments);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetInvoicePayments", new { id = invoicePayments.Id }, invoicePayments);
        }

        // DELETE: api/InvoicePayments/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteInvoicePayments(long id)
        {
            var invoicePayments = await _context.InvoicePayments.FindAsync(id);
            if (invoicePayments == null)
            {
                return NotFound();
            }

            _context.InvoicePayments.Remove(invoicePayments);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool InvoicePaymentsExists(long id)
        {
            return _context.InvoicePayments.Any(e => e.Id == id);
        }
    }
}