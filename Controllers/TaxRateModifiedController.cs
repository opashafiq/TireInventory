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
    public class TaxRateModifiedController : ControllerBase
    {
        private readonly ApplicationDBContext _context;

        public TaxRateModifiedController(ApplicationDBContext context)
        {
            _context = context;
        }

        // GET: api/TaxRateModified
        [HttpGet]
        public async Task<ActionResult<IEnumerable<TaxRateModified>>> GetTaxRateModified()
        {
            return await _context.TaxRateModifieds.ToListAsync();
        }

        // GET: api/TaxRateModified/5
        [HttpGet("{id}")]
        public async Task<ActionResult<TaxRateModified>> GetTaxRateModified(long id)
        {
            var taxRateModified = await _context.TaxRateModifieds.FindAsync(id);

            if (taxRateModified == null)
            {
                return NotFound();
            }

            return taxRateModified;
        }

        // PUT: api/TaxRateModified/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutTaxRateModified(long id, TaxRateModified taxRateModified)
        {
            if (id != taxRateModified.Id)
            {
                return BadRequest();
            }

            _context.Entry(taxRateModified).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!TaxRateModifiedExists(id))
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

        // POST: api/TaxRateModified
        [HttpPost]
        public async Task<ActionResult<TaxRateModified>> PostTaxRateModified(TaxRateModified taxRateModified)
        {
            _context.TaxRateModifieds.Add(taxRateModified);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetTaxRateModified", new { id = taxRateModified.Id }, taxRateModified);
        }

        // DELETE: api/TaxRateModified/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTaxRateModified(long id)
        {
            var taxRateModified = await _context.TaxRateModifieds.FindAsync(id);
            if (taxRateModified == null)
            {
                return NotFound();
            }

            _context.TaxRateModifieds.Remove(taxRateModified);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool TaxRateModifiedExists(long id)
        {
            return _context.TaxRateModifieds.Any(e => e.Id == id);
        }
    }
}