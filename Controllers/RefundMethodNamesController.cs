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
    public class RefundMethodNamesController : ControllerBase
    {
        private readonly ApplicationDBContext _context;

        public RefundMethodNamesController(ApplicationDBContext context)
        {
            _context = context;
        }

        // GET: api/RefundMethodNames
        [HttpGet]
        public async Task<ActionResult<IEnumerable<RefundMethodNames>>> GetRefundMethodNames()
        {
            return await _context.RefundMethodNames.ToListAsync();
        }

        // GET: api/RefundMethodNames/5
        [HttpGet("{id}")]
        public async Task<ActionResult<RefundMethodNames>> GetRefundMethodNames(long id)
        {
            var refundMethodNames = await _context.RefundMethodNames.FindAsync(id);

            if (refundMethodNames == null)
            {
                return NotFound();
            }

            return refundMethodNames;
        }

        // PUT: api/RefundMethodNames/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutRefundMethodNames(long id, RefundMethodNames refundMethodNames)
        {
            if (id != refundMethodNames.Id)
            {
                return BadRequest();
            }

            _context.Entry(refundMethodNames).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!RefundMethodNamesExists(id))
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

        // POST: api/RefundMethodNames
        [HttpPost]
        public async Task<ActionResult<RefundMethodNames>> PostRefundMethodNames(RefundMethodNames refundMethodNames)
        {
            _context.RefundMethodNames.Add(refundMethodNames);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetRefundMethodNames", new { id = refundMethodNames.Id }, refundMethodNames);
        }

        // DELETE: api/RefundMethodNames/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteRefundMethodNames(long id)
        {
            var refundMethodNames = await _context.RefundMethodNames.FindAsync(id);
            if (refundMethodNames == null)
            {
                return NotFound();
            }

            _context.RefundMethodNames.Remove(refundMethodNames);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool RefundMethodNamesExists(long id)
        {
            return _context.RefundMethodNames.Any(e => e.Id == id);
        }
    }
}