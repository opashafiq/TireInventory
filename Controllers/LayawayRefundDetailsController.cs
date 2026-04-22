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
    public class LayawayRefundDetailsController : ControllerBase
    {
        private readonly ApplicationDBContext _context;

        public LayawayRefundDetailsController(ApplicationDBContext context)
        {
            _context = context;
        }

        // GET: api/LayawayRefundDetails
        [HttpGet]
        public async Task<ActionResult<IEnumerable<LayawayRefundDetails>>> GetLayawayRefundDetails()
        {
            return await _context.LayawayRefundDetails.ToListAsync();
        }

        // GET: api/LayawayRefundDetails/5
        [HttpGet("{id}")]
        public async Task<ActionResult<LayawayRefundDetails>> GetLayawayRefundDetails(long id)
        {
            var layawayRefundDetails = await _context.LayawayRefundDetails.FindAsync(id);

            if (layawayRefundDetails == null)
            {
                return NotFound();
            }

            return layawayRefundDetails;
        }

        // PUT: api/LayawayRefundDetails/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutLayawayRefundDetails(long id, LayawayRefundDetails layawayRefundDetails)
        {
            if (id != layawayRefundDetails.Id)
            {
                return BadRequest();
            }

            _context.Entry(layawayRefundDetails).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!LayawayRefundDetailsExists(id))
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

        // POST: api/LayawayRefundDetails
        [HttpPost]
        public async Task<ActionResult<LayawayRefundDetails>> PostLayawayRefundDetails(LayawayRefundDetails layawayRefundDetails)
        {
            _context.LayawayRefundDetails.Add(layawayRefundDetails);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetLayawayRefundDetails", new { id = layawayRefundDetails.Id }, layawayRefundDetails);
        }

        // DELETE: api/LayawayRefundDetails/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteLayawayRefundDetails(long id)
        {
            var layawayRefundDetails = await _context.LayawayRefundDetails.FindAsync(id);
            if (layawayRefundDetails == null)
            {
                return NotFound();
            }

            _context.LayawayRefundDetails.Remove(layawayRefundDetails);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool LayawayRefundDetailsExists(long id)
        {
            return _context.LayawayRefundDetails.Any(e => e.Id == id);
        }
    }
}