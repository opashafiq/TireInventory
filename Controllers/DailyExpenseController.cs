using System.Linq;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using TireInventory.Data;
using TireInventory.Helpers;
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

            // Load original values for audit
            var original = await _context.DailyExpenses.AsNoTracking().FirstOrDefaultAsync(d => d.Id == id);
            if (original == null) return NotFound();

            // mark entity modified and save
            _context.Entry(dailyExpense).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();

                // Create audit trail entry
                var audit = new AuditTrail
                {
                    Event = "Update DailyExpense",
                    EventType = AuditEventType.Update,
                    EntityName = "DailyExpense",
                    EntityId = dailyExpense.Id.ToString(),
                    Changes = JsonSerializer.Serialize(new { Old = original, New = dailyExpense }),
                    Description = $"Updated DailyExpense Id={dailyExpense.Id}",
                    IsSuccess = true,
                    Metadata = JsonSerializer.Serialize(new
                    {
                        ExpenseHeadId = dailyExpense.ExpenseHeadId,
                        Amount = dailyExpense.Amount,
                        ExpenseDate = dailyExpense.ExpenseDate,
                        LocationDetailsId = dailyExpense.LocationDetailsId,
                        RequestPath = Request.Path,
                        CorrelationId = Request.Headers["X-Correlation-ID"].ToString()
                    }),
                    PerformedByName = dailyExpense.UserName ?? original.UserName,
                    PerformedById = dailyExpense.UserName ?? original.UserName,
                    IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString(),
                    UserAgent = Request.Headers["User-Agent"].ToString()
                };

                _context.AuditTrails.Add(audit);
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

            // Audit - create
            var auditCreate = new AuditTrail
            {
                Event = "Create DailyExpense",
                EventType = AuditEventType.Create,
                EntityName = "DailyExpense",
                EntityId = dailyExpense.Id.ToString(),
                Changes = JsonSerializer.Serialize(new { New = dailyExpense }),
                Description = $"Created DailyExpense Id={dailyExpense.Id}",
                IsSuccess = true,
                Metadata = JsonSerializer.Serialize(new
                {
                    ExpenseHeadId = dailyExpense.ExpenseHeadId,
                    Amount = dailyExpense.Amount,
                    ExpenseDate = dailyExpense.ExpenseDate,
                    LocationDetailsId = dailyExpense.LocationDetailsId,
                    RequestPath = Request.Path,
                    CorrelationId = Request.Headers["X-Correlation-ID"].ToString()
                }),
                PerformedByName = dailyExpense.UserName,
                PerformedById = dailyExpense.UserName,
                IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString(),
                UserAgent = Request.Headers["User-Agent"].ToString()
            };

            _context.AuditTrails.Add(auditCreate);
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

            try
            {
                // capture for audit
                var auditDelete = new AuditTrail
                {
                    Event = "Delete DailyExpense",
                    EventType = AuditEventType.Delete,
                    EntityName = "DailyExpense",
                    EntityId = dailyExpense.Id.ToString(),
                    Changes = JsonSerializer.Serialize(new { Old = dailyExpense }),
                    Description = $"Deleted DailyExpense Id={dailyExpense.Id}",
                    IsSuccess = true,
                    Metadata = JsonSerializer.Serialize(new
                    {
                        ExpenseHeadId = dailyExpense.ExpenseHeadId,
                        Amount = dailyExpense.Amount,
                        ExpenseDate = dailyExpense.ExpenseDate,
                        LocationDetailsId = dailyExpense.LocationDetailsId,
                        RequestPath = Request.Path,
                        CorrelationId = Request.Headers["X-Correlation-ID"].ToString()
                    }),
                    PerformedByName = dailyExpense.UserName,
                    PerformedById = dailyExpense.UserName,
                    IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString(),
                    UserAgent = Request.Headers["User-Agent"].ToString()
                };

                _context.DailyExpenses.Remove(dailyExpense);
                await _context.SaveChangesAsync();

                _context.AuditTrails.Add(auditDelete);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                if (ex.InnerException.Message.Contains("DELETE statement conflicted with the REFERENCE constraint"))
                {
                    return StatusCode(409, new { message = Messages.GetRefKeyErrorMessage("DailyExpense", ""), error = ex.InnerException?.Message ?? ex.Message });
                }
                return StatusCode(500, new { message = "An error occurred during deletion of daily expense", error = ex.InnerException?.Message ?? ex.Message });
            }

            return NoContent();
        }

        private bool DailyExpenseExists(long id)
        {
            return _context.DailyExpenses.Any(e => e.Id == id);
        }
    }
}