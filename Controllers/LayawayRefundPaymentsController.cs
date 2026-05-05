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
    public class LayawayRefundPaymentsController : ControllerBase
    {
        private readonly ApplicationDBContext _context;

        public LayawayRefundPaymentsController(ApplicationDBContext context)
        {
            _context = context;
        }

        // GET: api/LayawayRefundPayments
        [HttpGet]
        public async Task<ActionResult<IEnumerable<LayawayRefundPaymentsDto>>> GetLayawayRefundPayments()
        {
            var list = await (from p in _context.LayawayRefundPayments
                              join rm in _context.RefundMethodNames
                                  on p.tbirp_RefundMethodId equals rm.Id into gj
                              from rm in gj.DefaultIfEmpty()
                              select new LayawayRefundPaymentsDto
                              {
                                  Id = p.Id,
                                  tbirp_Layaway_RefundId = p.tbirp_Layaway_RefundId,
                                  tbirp_RefundMethodId = p.tbirp_RefundMethodId,
                                  tbirp_RefundAmt = p.tbirp_RefundAmt,
                                  tbirp_Date = p.tbirp_Date,
                                  RefundMethodName = rm != null ? rm.tbrmn_RefundMethodName : string.Empty
                              })
                             .ToListAsync();

            return Ok(list);
        }

        // GET: api/LayawayRefundPayments/5
        [HttpGet("{id}")]
        public async Task<ActionResult<LayawayRefundPaymentsDto>> GetLayawayRefundPayments(long id)
        {
            var dto = await (from p in _context.LayawayRefundPayments
                             join rm in _context.RefundMethodNames
                                 on p.tbirp_RefundMethodId equals rm.Id into gj
                             from rm in gj.DefaultIfEmpty()
                             where p.Id == id
                             select new LayawayRefundPaymentsDto
                             {
                                 Id = p.Id,
                                 tbirp_Layaway_RefundId = p.tbirp_Layaway_RefundId,
                                 tbirp_RefundMethodId = p.tbirp_RefundMethodId,
                                 tbirp_RefundAmt = p.tbirp_RefundAmt,
                                 tbirp_Date = p.tbirp_Date,
                                 RefundMethodName = rm != null ? rm.tbrmn_RefundMethodName : string.Empty
                             })
                            .FirstOrDefaultAsync();

            if (dto == null) return NotFound();
            return Ok(dto);
        }

        // PUT: api/LayawayRefundPayments/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutLayawayRefundPayments(long id, LayawayRefundPayments layawayRefundPayments)
        {
            if (id != layawayRefundPayments.Id)
            {
                return BadRequest();
            }

            _context.Entry(layawayRefundPayments).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!LayawayRefundPaymentsExists(id))
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

        // POST: api/LayawayRefundPayments
        [HttpPost]
        public async Task<ActionResult<LayawayRefundPayments>> PostLayawayRefundPayments(LayawayRefundPayments layawayRefundPayments)
        {
            _context.LayawayRefundPayments.Add(layawayRefundPayments);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetLayawayRefundPayments", new { id = layawayRefundPayments.Id }, layawayRefundPayments);
        }

        // DELETE: api/LayawayRefundPayments/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteLayawayRefundPayments(long id)
        {
            var layawayRefundPayments = await _context.LayawayRefundPayments.FindAsync(id);
            if (layawayRefundPayments == null)
            {
                return NotFound();
            }

            _context.LayawayRefundPayments.Remove(layawayRefundPayments);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool LayawayRefundPaymentsExists(long id)
        {
            return _context.LayawayRefundPayments.Any(e => e.Id == id);
        }
    }
}