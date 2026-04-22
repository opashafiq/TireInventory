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
    public class LayawayDetailsController : ControllerBase
    {
        private readonly ApplicationDBContext _context;

        public LayawayDetailsController(ApplicationDBContext context)
        {
            _context = context;
        }

        // GET: api/LayawayDetails
        [HttpGet]
        public async Task<ActionResult<IEnumerable<LayawayDetails>>> GetLayawayDetails()
        {
            return await _context.LayawayDetails.ToListAsync();
        }

        // GET: api/LayawayDetails/5
        [HttpGet("{id}")]
        public async Task<ActionResult<LayawayDetails>> GetLayawayDetails(long id)
        {
            var layawayDetails = await _context.LayawayDetails.FindAsync(id);

            if (layawayDetails == null)
            {
                return NotFound();
            }

            return layawayDetails;
        }

        // PUT: api/LayawayDetails/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutLayawayDetails(long id, LayawayDetails layawayDetails)
        {
            if (id != layawayDetails.Id)
            {
                return BadRequest();
            }

            _context.Entry(layawayDetails).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!LayawayDetailsExists(id))
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

        // POST: api/LayawayDetails
        [HttpPost]
        public async Task<ActionResult<LayawayDetails>> PostLayawayDetails(LayawayDetails layawayDetails)
        {
            _context.LayawayDetails.Add(layawayDetails);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetLayawayDetails", new { id = layawayDetails.Id }, layawayDetails);
        }

        // DELETE: api/LayawayDetails/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteLayawayDetails(long id)
        {
            var layawayDetails = await _context.LayawayDetails.FindAsync(id);
            if (layawayDetails == null)
            {
                return NotFound();
            }

            _context.LayawayDetails.Remove(layawayDetails);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool LayawayDetailsExists(long id)
        {
            return _context.LayawayDetails.Any(e => e.Id == id);
        }
    }
}