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
    public class LayawayPaymentsController : ControllerBase
    {
        private readonly ApplicationDBContext _context;

        public LayawayPaymentsController(ApplicationDBContext context)
        {
            _context = context;
        }

        // GET: api/LayawayPayments
        [HttpGet]
        public async Task<ActionResult<IEnumerable<LayawayPaymentsDto>>> GetLayawayPayments()
        {
            var list = await (from p in _context.LayawayPayments
                              join pay in _context.PaymentNames
                                  on p.tbip_PaymentId equals pay.Id into gj
                              from pay in gj.DefaultIfEmpty()
                              select new LayawayPaymentsDto
                              {
                                  Id = p.Id,
                                  tbip_InvoiceId = p.tbip_InvoiceId,
                                  tbip_PaymentId = p.tbip_PaymentId,
                                  tbip_PayAmt = p.tbip_PayAmt,
                                  tbip_Date = p.tbip_Date,
                                  tbip_PaymentType = p.tbip_PaymentType,
                                  PaymentName = pay != null ? pay.tbpn_PaymentName : string.Empty
                              })
                             .ToListAsync();

            return Ok(list);
        }

        // GET: api/LayawayPayments/5
        [HttpGet("{id}")]
        public async Task<ActionResult<LayawayPaymentsDto>> GetLayawayPayments(long id)
        {
            var dto = await (from p in _context.LayawayPayments
                             join pay in _context.PaymentNames
                                 on p.tbip_PaymentId equals pay.Id into gj
                             from pay in gj.DefaultIfEmpty()
                             where p.Id == id
                             select new LayawayPaymentsDto
                             {
                                 Id = p.Id,
                                 tbip_InvoiceId = p.tbip_InvoiceId,
                                 tbip_PaymentId = p.tbip_PaymentId,
                                 tbip_PayAmt = p.tbip_PayAmt,
                                 tbip_Date = p.tbip_Date,
                                 tbip_PaymentType = p.tbip_PaymentType,
                                 PaymentName = pay != null ? pay.tbpn_PaymentName : string.Empty
                             })
                            .FirstOrDefaultAsync();

            if (dto == null) return NotFound();
            return Ok(dto);
        }

        // PUT: api/LayawayPayments/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutLayawayPayments(long id, LayawayPayments layawayPayments)
        {
            if (id != layawayPayments.Id)
            {
                return BadRequest();
            }

            _context.Entry(layawayPayments).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!LayawayPaymentsExists(id))
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

        // POST: api/LayawayPayments
        [HttpPost]
        public async Task<ActionResult<LayawayPayments>> PostLayawayPayments(LayawayPayments layawayPayments)
        {
            _context.LayawayPayments.Add(layawayPayments);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetLayawayPayments", new { id = layawayPayments.Id }, layawayPayments);
        }

        // DELETE: api/LayawayPayments/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteLayawayPayments(long id)
        {
            var layawayPayments = await _context.LayawayPayments.FindAsync(id);
            if (layawayPayments == null)
            {
                return NotFound();
            }

            _context.LayawayPayments.Remove(layawayPayments);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool LayawayPaymentsExists(long id)
        {
            return _context.LayawayPayments.Any(e => e.Id == id);
        }
    }
}