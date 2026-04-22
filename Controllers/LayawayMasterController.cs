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
    public class LayawayMasterController : ControllerBase
    {
        private readonly ApplicationDBContext _context;

        public LayawayMasterController(ApplicationDBContext context)
        {
            _context = context;
        }

        // GET: api/LayawayMaster
        [HttpGet]
        public async Task<ActionResult<IEnumerable<LayawayMaster>>> GetLayawayMasters()
        {
            return await _context.LayawayMasters.ToListAsync();
        }

        // GET: api/LayawayMaster/5
        [HttpGet("{id}")]
        public async Task<ActionResult<LayawayMaster>> GetLayawayMaster(long id)
        {
            var layawayMaster = await _context.LayawayMasters.FindAsync(id);

            if (layawayMaster == null)
            {
                return NotFound();
            }

            return layawayMaster;
        }

        // PUT: api/LayawayMaster/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutLayawayMaster(long id, LayawayMaster layawayMaster)
        {
            if (id != layawayMaster.Id)
            {
                return BadRequest();
            }

            _context.Entry(layawayMaster).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!LayawayMasterExists(id))
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

        // POST: api/LayawayMaster
        [HttpPost]
        public async Task<ActionResult<LayawayMaster>> PostLayawayMaster(LayawayMaster layawayMaster)
        {
            _context.LayawayMasters.Add(layawayMaster);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetLayawayMaster", new { id = layawayMaster.Id }, layawayMaster);
        }

        // DELETE: api/LayawayMaster/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteLayawayMaster(long id)
        {
            var layawayMaster = await _context.LayawayMasters.FindAsync(id);
            if (layawayMaster == null)
            {
                return NotFound();
            }

            _context.LayawayMasters.Remove(layawayMaster);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool LayawayMasterExists(long id)
        {
            return _context.LayawayMasters.Any(e => e.Id == id);
        }
    }
}