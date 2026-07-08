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
    public class ItemMasterController : ControllerBase
    {
        private readonly ApplicationDBContext _context;

        public ItemMasterController(ApplicationDBContext context)
        {
            _context = context;
        }

        // GET: api/ItemMaster
        [HttpGet]
        //public async Task<ActionResult<IEnumerable<ItemMaster>>> GetItemMasters()
        //{
        //    return await _context.ItemMasters.ToListAsync();
        //}

        public async Task<ActionResult<IEnumerable<ItemMasterDto>>> GetItemMasters()
        {
            var list = await (from im in _context.ItemMasters
                              join dep in _context.Departments
                                  on im.tbim_ItemCategoryId equals dep.Id
                              join dis in _context.Distributors
                                    on im.tbim_DistributorId equals dis.Id
                              join loc in _context.LocationDetails
                                    on im.tbim_LocationId equals loc.Id
                              select new ItemMasterDto
                              {
                                  Id = im.Id,
                                  tbim_ItemCategoryId = im.tbim_ItemCategoryId,
                                  tbim_Size = im.tbim_Size,
                                  tbim_Brand = im.tbim_Brand,
                                  tbim_Series = im.tbim_Series,
                                  tbim_Bolt = im.tbim_Bolt,
                                  tbim_HoleS = im.tbim_HoleS,
                                  tbim_Zone = im.tbim_Zone,
                                  tbim_Qty = im.tbim_Qty,
                                  tbim_QtyOp = im.tbim_QtyOp,
                                  tbim_Code = im.tbim_Code,
                                  tbim_CodeTOT = im.tbim_CodeTOT,
                                  tbim_DistributorId = im.tbim_DistributorId,
                                  tbim_OURP = im.tbim_OURP,
                                  tbim_LocationId = im.tbim_LocationId,
                                  DepartmentName = dep.Tbid_DepartmentName,
                                  DistributorName = dis.Name,
                                  LocationName = loc.tbld_LocationName,
                                  tbim_ThrashDate = im.tbim_ThrashDate,
                                  UserName = im.UserName,
                                  SetDate = im.SetDate

                              })
                             .ToListAsync();

            return Ok(list);
        }

        // GET: api/ItemMaster/5
        [HttpGet("{id}")]
        public async Task<ActionResult<ItemMasterDto>> GetItemMaster(long id)
        {
            var dto = await (from im in _context.ItemMasters
                             join dep in _context.Departments
                                 on im.tbim_ItemCategoryId equals dep.Id
                             join dis in _context.Distributors
                                   on im.tbim_DistributorId equals dis.Id
                             join loc in _context.LocationDetails
                                   on im.tbim_LocationId equals loc.Id
                             where im.Id == id
                             select new ItemMasterDto
                             {
                                 Id = im.Id,
                                 tbim_ItemCategoryId = im.tbim_ItemCategoryId,
                                 tbim_Size = im.tbim_Size,
                                 tbim_Brand = im.tbim_Brand,
                                 tbim_Series = im.tbim_Series,
                                 tbim_Bolt = im.tbim_Bolt,
                                 tbim_HoleS = im.tbim_HoleS,
                                 tbim_Zone = im.tbim_Zone,
                                 tbim_Qty = im.tbim_Qty,
                                 tbim_QtyOp = im.tbim_QtyOp,
                                 tbim_Code = im.tbim_Code,
                                 tbim_CodeTOT = im.tbim_CodeTOT,
                                 tbim_DistributorId = im.tbim_DistributorId,
                                 tbim_OURP = im.tbim_OURP,
                                 tbim_LocationId = im.tbim_LocationId,
                                 DepartmentName = dep.Tbid_DepartmentName,
                                 DistributorName = dis.Name,
                                 LocationName = loc.tbld_LocationName,
                                 tbim_ThrashDate = im.tbim_ThrashDate,
                                 UserName = im.UserName,
                                 SetDate = im.SetDate
                             })
                            .FirstOrDefaultAsync();

            if (dto == null) return NotFound();
            return Ok(dto);
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
        public async Task<ActionResult<ItemMasterDto>> PostItemMaster(ItemMaster itemMaster)
        {
            _context.ItemMasters.Add(itemMaster);
            await _context.SaveChangesAsync();

            return await GetItemMaster(itemMaster.Id);
            //return CreatedAtAction("GetItemMaster", new { id = itemMaster.Id }, itemMaster);
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