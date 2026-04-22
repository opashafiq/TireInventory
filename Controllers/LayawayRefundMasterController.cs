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
    public class LayawayRefundMasterController : ControllerBase
    {
        private readonly ApplicationDBContext _context;

        public LayawayRefundMasterController(ApplicationDBContext context)
        {
            _context = context;
        }

        // GET: api/LayawayRefundMaster
        [HttpGet]
        public async Task<ActionResult<IEnumerable<LayawayRefundMaster>>> GetLayawayRefundMasters()
        {
            return await _context.LayawayRefundMasters.ToListAsync();
        }

        // GET: api/LayawayRefundMaster/5
        [HttpGet("{id}")]
        public async Task<ActionResult<LayawayRefundMaster>> GetLayawayRefundMaster(long id)
        {
            var layawayRefundMaster = await _context.LayawayRefundMasters.FindAsync(id);

            if (layawayRefundMaster == null)
            {
                return NotFound();
            }

            return layawayRefundMaster;
        }

        // PUT: api/LayawayRefundMaster/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutLayawayRefundMaster(long id, LayawayRefundMaster layawayRefundMaster)
        {
            if (id != layawayRefundMaster.Id)
            {
                return BadRequest();
            }

            _context.Entry(layawayRefundMaster).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!LayawayRefundMasterExists(id))
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

        // POST: api/LayawayRefundMaster
        [HttpPost]
        public async Task<ActionResult<LayawayRefundMaster>> PostLayawayRefundMaster(LayawayRefundMaster layawayRefundMaster)
        {
            _context.LayawayRefundMasters.Add(layawayRefundMaster);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetLayawayRefundMaster", new { id = layawayRefundMaster.Id }, layawayRefundMaster);
        }

        // DELETE: api/LayawayRefundMaster/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteLayawayRefundMaster(long id)
        {
            var layawayRefundMaster = await _context.LayawayRefundMasters.FindAsync(id);
            if (layawayRefundMaster == null)
            {
                return NotFound();
            }

            _context.LayawayRefundMasters.Remove(layawayRefundMaster);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool LayawayRefundMasterExists(long id)
        {
            return _context.LayawayRefundMasters.Any(e => e.Id == id);
        }
    }
}