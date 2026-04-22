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
    public class LayawayRefundPaymentsController : ControllerBase
    {
        private readonly ApplicationDBContext _context;

        public LayawayRefundPaymentsController(ApplicationDBContext context)
        {
            _context = context;
        }

        // GET: api/LayawayRefundPayments
        [HttpGet]
        public async Task<ActionResult<IEnumerable<LayawayRefundPayments>>> GetLayawayRefundPayments()
        {
            return await _context.LayawayRefundPayments.ToListAsync();
        }

        // GET: api/LayawayRefundPayments/5
        [HttpGet("{id}")]
        public async Task<ActionResult<LayawayRefundPayments>> GetLayawayRefundPayments(long id)
        {
            var layawayRefundPayments = await _context.LayawayRefundPayments.FindAsync(id);

            if (layawayRefundPayments == null)
            {
                return NotFound();
            }

            return layawayRefundPayments;
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