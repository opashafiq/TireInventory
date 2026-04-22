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
    public class ExpenseHeadController : ControllerBase
    {
        private readonly ApplicationDBContext _context;

        public ExpenseHeadController(ApplicationDBContext context)
        {
            _context = context;
        }

        // GET: api/ExpenseHead
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ExpenseHead>>> GetExpenseHeads()
        {
            return await _context.ExpenseHeads.ToListAsync();
        }

        // GET: api/ExpenseHead/5
        [HttpGet("{id}")]
        public async Task<ActionResult<ExpenseHead>> GetExpenseHead(long id)
        {
            var expenseHead = await _context.ExpenseHeads.FindAsync(id);

            if (expenseHead == null)
            {
                return NotFound();
            }

            return expenseHead;
        }

        // PUT: api/ExpenseHead/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutExpenseHead(long id, ExpenseHead expenseHead)
        {
            if (id != expenseHead.Id)
            {
                return BadRequest();
            }

            _context.Entry(expenseHead).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ExpenseHeadExists(id))
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

        // POST: api/ExpenseHead
        [HttpPost]
        public async Task<ActionResult<ExpenseHead>> PostExpenseHead(ExpenseHead expenseHead)
        {
            _context.ExpenseHeads.Add(expenseHead);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetExpenseHead", new { id = expenseHead.Id }, expenseHead);
        }

        // DELETE: api/ExpenseHead/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteExpenseHead(long id)
        {
            var expenseHead = await _context.ExpenseHeads.FindAsync(id);
            if (expenseHead == null)
            {
                return NotFound();
            }

            _context.ExpenseHeads.Remove(expenseHead);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool ExpenseHeadExists(long id)
        {
            return _context.ExpenseHeads.Any(e => e.Id == id);
        }
    }
}