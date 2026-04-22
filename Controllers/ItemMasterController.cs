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
    public class ItemMasterController : ControllerBase
    {
        private readonly ApplicationDBContext _context;

        public ItemMasterController(ApplicationDBContext context)
        {
            _context = context;
        }

        // GET: api/ItemMaster
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ItemMaster>>> GetItemMasters()
        {
            return await _context.ItemMasters.ToListAsync();
        }

        // GET: api/ItemMaster/5
        [HttpGet("{id}")]
        public async Task<ActionResult<ItemMaster>> GetItemMaster(long id)
        {
            var itemMaster = await _context.ItemMasters.FindAsync(id);

            if (itemMaster == null)
            {
                return NotFound();
            }

            return itemMaster;
        }

        // PUT: api/ItemMaster/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutItemMaster(long id, ItemMaster itemMaster)
        {
            if (id != itemMaster.Id)
            {
                return BadRequest();
            }

            _context.Entry(itemMaster).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ItemMasterExists(id))
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

        // POST: api/ItemMaster
        [HttpPost]
        public async Task<ActionResult<ItemMaster>> PostItemMaster(ItemMaster itemMaster)
        {
            _context.ItemMasters.Add(itemMaster);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetItemMaster", new { id = itemMaster.Id }, itemMaster);
        }

        // DELETE: api/ItemMaster/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteItemMaster(long id)
        {
            var itemMaster = await _context.ItemMasters.FindAsync(id);
            if (itemMaster == null)
            {
                return NotFound();
            }

            _context.ItemMasters.Remove(itemMaster);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool ItemMasterExists(long id)
        {
            return _context.ItemMasters.Any(e => e.Id == id);
        }
    }
}