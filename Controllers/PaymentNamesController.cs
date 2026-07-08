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
    public class PaymentNamesController : ControllerBase
    {
        private readonly ApplicationDBContext _context;

        public PaymentNamesController(ApplicationDBContext context)
        {
            _context = context;
        }

        // GET: api/PaymentNames
        [HttpGet]
        public async Task<ActionResult<IEnumerable<PaymentNames>>> GetPaymentNames()
        {
            return await _context.PaymentNames.ToListAsync();
        }

        // GET: api/PaymentNames/5
        [HttpGet("{id}")]
        public async Task<ActionResult<PaymentNames>> GetPaymentNames(long id)
        {
            var paymentNames = await _context.PaymentNames.FindAsync(id);

            if (paymentNames == null)
            {
                return NotFound();
            }

            return paymentNames;
        }

        // PUT: api/PaymentNames/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutPaymentNames(long id, PaymentNames paymentNames)
        {
            if (id != paymentNames.Id)
            {
                return BadRequest();
            }

            _context.Entry(paymentNames).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!PaymentNamesExists(id))
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

        // POST: api/PaymentNames
        [HttpPost]
        public async Task<ActionResult<PaymentNames>> PostPaymentNames(PaymentNames paymentNames)
        {
            _context.PaymentNames.Add(paymentNames);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetPaymentNames", new { id = paymentNames.Id }, paymentNames);
        }

        // DELETE: api/PaymentNames/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePaymentNames(long id)
        {
            var paymentNames = await _context.PaymentNames.FindAsync(id);
            if (paymentNames == null)
            {
                return NotFound();
            }

            _context.PaymentNames.Remove(paymentNames);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool PaymentNamesExists(long id)
        {
            return _context.PaymentNames.Any(e => e.Id == id);
        }
    }
}