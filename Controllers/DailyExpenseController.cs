using System.Linq;
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
    public class DailyExpenseController : ControllerBase
    {
        private readonly ApplicationDBContext _context;

        public DailyExpenseController(ApplicationDBContext context)
        {
            _context = context;
        }

        // GET: api/DailyExpense
        [HttpGet]
        public async Task<ActionResult<IEnumerable<DailyExpenseDto>>> GetDailyExpense()
        {
            var list = await (from de in _context.DailyExpenses
                              join eh in _context.ExpenseHeads
                                  on de.ExpenseHeadId equals eh.Id
                              join ld in _context.LocationDetails
                                    on de.LocationDetailsId equals ld.Id
                              select new DailyExpenseDto
                              {
                                  Id = de.Id,
                                  ExpenseHeadId = de.ExpenseHeadId,
                                  ExpenseDate = de.ExpenseDate,
                                  Amount = de.Amount,
                                  CheckNo = de.CheckNo,
                                  PayType = de.PayType,
                                  UserName = de.UserName,
                                  SetDate = de.SetDate,
                                  LocationDetailsId = de.LocationDetailsId,
                                  ExpenseHeadName = eh.tbeh_HeadName,
                                  LocationDetailsName = ld.tbld_LocationName
                              })
                             .ToListAsync();

            return Ok(list);
        }

        // GET: api/DailyExpense/5
        [HttpGet("{id}")]
        public async Task<ActionResult<DailyExpenseDto>> GetDailyExpense(long id)
        {
            var dto = await (from de in _context.DailyExpenses
                             join eh in _context.ExpenseHeads
                                 on de.ExpenseHeadId equals eh.Id
                             join ld in _context.LocationDetails
                                   on de.LocationDetailsId equals ld.Id
                             where de.Id == id
                             select new DailyExpenseDto
                             {
                                 Id = de.Id,
                                 ExpenseHeadId = de.ExpenseHeadId,
                                 ExpenseDate = de.ExpenseDate,
                                 Amount = de.Amount,
                                 CheckNo = de.CheckNo,
                                 PayType = de.PayType,
                                 UserName = de.UserName,
                                 SetDate = de.SetDate,
                                 LocationDetailsId = de.LocationDetailsId,
                                 ExpenseHeadName = eh.tbeh_HeadName,
                                 LocationDetailsName = ld.tbld_LocationName
                             })
                            .FirstOrDefaultAsync();

            if (dto == null) return NotFound();
            return Ok(dto);
        }

        // PUT: api/DailyExpense/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutDailyExpense(long id, DailyExpense dailyExpense)
        {
            if (id != dailyExpense.Id)
            {
                return BadRequest();
            }

            _context.Entry(dailyExpense).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!DailyExpenseExists(id))
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

        // POST: api/DailyExpense
        [HttpPost]
        public async Task<ActionResult<DailyExpenseDto>> PostDailyExpense(DailyExpense dailyExpense)
        {
            _context.DailyExpenses.Add(dailyExpense);
            await _context.SaveChangesAsync();

            return await GetDailyExpense(dailyExpense.Id);
        }

        // DELETE: api/DailyExpense/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteDailyExpense(long id)
        {
            var dailyExpense = await _context.DailyExpenses.FindAsync(id);
            if (dailyExpense == null)
            {
                return NotFound();
            }

            _context.DailyExpenses.Remove(dailyExpense);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool DailyExpenseExists(long id)
        {
            return _context.DailyExpenses.Any(e => e.Id == id);
        }
    }
}