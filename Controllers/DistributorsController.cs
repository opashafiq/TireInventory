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
    public class DistributorsController : ControllerBase
    {
        // Test comment for git commit
        private readonly ApplicationDBContext _context;

        public DistributorsController(ApplicationDBContext context)
        {
            _context = context;
        }

        // GET: api/Distributors
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Distributors>>> GetDistributors()
        {
            return await _context.Distributors.ToListAsync();
        }

        // GET: api/Distributors/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Distributors>> GetDistributors(int id)
        {
            var distributors = await _context.Distributors.FindAsync(id);

            if (distributors == null)
            {
                return NotFound();
            }

            return distributors;
        }

        // PUT: api/Distributors/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutDistributors(int id, Distributors distributors)
        {
            if (id != distributors.Id)
            {
                return BadRequest();
            }

            _context.Entry(distributors).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!DistributorsExists(id))
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

        // POST: api/Distributors
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<Distributors>> PostDistributors(Distributors distributors)
        {
            _context.Distributors.Add(distributors);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetDistributors", new { id = distributors.Id }, distributors);
        }

        // DELETE: api/Distributors/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteDistributors(int id)
        {
            var distributors = await _context.Distributors.FindAsync(id);
            if (distributors == null)
            {
                return NotFound();
            }

            _context.Distributors.Remove(distributors);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool DistributorsExists(int id)
        {
            return _context.Distributors.Any(e => e.Id == id);
        }
    }
}
