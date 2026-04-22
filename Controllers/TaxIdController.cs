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
    public class TaxIdController : ControllerBase
    {
        private readonly ApplicationDBContext _context;

        public TaxIdController(ApplicationDBContext context)
        {
            _context = context;
        }

        // GET: api/TaxId
        [HttpGet]
        public async Task<ActionResult<IEnumerable<TaxId>>> GetTaxIds()
        {
            return await _context.TaxIds.ToListAsync();
        }

        // GET: api/TaxId/5
        [HttpGet("{id}")]
        public async Task<ActionResult<TaxId>> GetTaxId(long id)
        {
            var taxId = await _context.TaxIds.FindAsync(id);

            if (taxId == null)
            {
                return NotFound();
            }

            return taxId;
        }

        // PUT: api/TaxId/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutTaxId(long id, TaxId taxId)
        {
            if (id != taxId.Id)
            {
                return BadRequest();
            }

            _context.Entry(taxId).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!TaxIdExists(id))
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

        // POST: api/TaxId
        [HttpPost]
        public async Task<ActionResult<TaxId>> PostTaxId(TaxId taxId)
        {
            _context.TaxIds.Add(taxId);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetTaxId", new { id = taxId.Id }, taxId);
        }

        // DELETE: api/TaxId/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTaxId(long id)
        {
            var taxId = await _context.TaxIds.FindAsync(id);
            if (taxId == null)
            {
                return NotFound();
            }

            _context.TaxIds.Remove(taxId);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool TaxIdExists(long id)
        {
            return _context.TaxIds.Any(e => e.Id == id);
        }
    }
}