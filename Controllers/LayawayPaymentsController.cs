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
    public class LayawayPaymentsController : ControllerBase
    {
        private readonly ApplicationDBContext _context;

        public LayawayPaymentsController(ApplicationDBContext context)
        {
            _context = context;
        }

        // GET: api/LayawayPayments
        [HttpGet]
        public async Task<ActionResult<IEnumerable<LayawayPayments>>> GetLayawayPayments()
        {
            return await _context.LayawayPayments.ToListAsync();
        }

        // GET: api/LayawayPayments/5
        [HttpGet("{id}")]
        public async Task<ActionResult<LayawayPayments>> GetLayawayPayments(long id)
        {
            var layawayPayments = await _context.LayawayPayments.FindAsync(id);

            if (layawayPayments == null)
            {
                return NotFound();
            }

            return layawayPayments;
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