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
    public class LayawayRefundDetailsController : ControllerBase
    {
        private readonly ApplicationDBContext _context;

        public LayawayRefundDetailsController(ApplicationDBContext context)
        {
            _context = context;
        }

        // GET: api/LayawayRefundDetails
        [HttpGet]
        public async Task<ActionResult<IEnumerable<LayawayRefundDetailsDto>>> GetLayawayRefundDetails()
        {
            var list = await _context.LayawayRefundDetails
                .Select(d => new LayawayRefundDetailsDto
                {
                    Id = d.Id,
                    tbird_Layaway_RefundId = d.tbird_Layaway_RefundId,
                    tbird_ItemId = d.tbird_ItemId,
                    tbird_ItemCategory = d.tbird_ItemCategory,
                    tbird_DepartmentName = d.tbird_DepartmentName,
                    tbird_Size = d.tbird_Size,
                    tbird_Brand = d.tbird_Brand,
                    tbird_Series = d.tbird_Series,
                    tbird_Bolt = d.tbird_Bolt,
                    tbird_HoleS = d.tbird_HoleS,
                    tbird_Zone = d.tbird_Zone,
                    tbird_DistributorId = d.tbird_DistributorId,
                    tbird_DistributorName = d.tbird_DistributorName,
                    tbird_Qty = d.tbird_Qty,
                    tbird_Layaway_Qty = d.tbird_Layaway_Qty,
                    tbird_Layaway_Qty_LineTotal = d.tbird_Layaway_Qty_LineTotal,
                    tbird_Layaway_Qty_TaxAmt = d.tbird_Layaway_Qty_TaxAmt,
                    tbird_Taxable = d.tbird_Taxable,
                    tbird_UnitPrice = d.tbird_UnitPrice,
                    tbird_LineTotal = d.tbird_LineTotal,
                    tbird_TaxRate = d.tbird_TaxRate,
                    tbird_TaxAmt = d.tbird_TaxAmt,

                    ItemDepartmentName = d.tbird_Item != null && d.tbird_Item.tbim_ItemCategory != null
                        ? d.tbird_Item.tbim_ItemCategory.Tbid_DepartmentName : string.Empty,
                    ItemDistributorName = d.tbird_Item != null && d.tbird_Item.tbim_Distributor != null
                        ? d.tbird_Item.tbim_Distributor.Name : string.Empty,
                    ItemLocationName = d.tbird_Item != null && d.tbird_Item.tbim_Location != null
                        ? d.tbird_Item.tbim_Location.tbld_LocationName : string.Empty,
                    ItemDisplay = d.tbird_Item != null
                        ? (d.tbird_Item.tbim_Brand + " " + d.tbird_Item.tbim_Size).Trim() : string.Empty
                })
                .ToListAsync();

            return Ok(list);
        }

        // GET: api/LayawayRefundDetails/5
        [HttpGet("{id}")]
        public async Task<ActionResult<LayawayRefundDetailsDto>> GetLayawayRefundDetails(long id)
        {
            var dto = await _context.LayawayRefundDetails
                .Where(d => d.Id == id)
                .Select(d => new LayawayRefundDetailsDto
                {
                    Id = d.Id,
                    tbird_Layaway_RefundId = d.tbird_Layaway_RefundId,
                    tbird_ItemId = d.tbird_ItemId,
                    tbird_ItemCategory = d.tbird_ItemCategory,
                    tbird_DepartmentName = d.tbird_DepartmentName,
                    tbird_Size = d.tbird_Size,
                    tbird_Brand = d.tbird_Brand,
                    tbird_Series = d.tbird_Series,
                    tbird_Bolt = d.tbird_Bolt,
                    tbird_HoleS = d.tbird_HoleS,
                    tbird_Zone = d.tbird_Zone,
                    tbird_DistributorId = d.tbird_DistributorId,
                    tbird_DistributorName = d.tbird_DistributorName,
                    tbird_Qty = d.tbird_Qty,
                    tbird_Layaway_Qty = d.tbird_Layaway_Qty,
                    tbird_Layaway_Qty_LineTotal = d.tbird_Layaway_Qty_LineTotal,
                    tbird_Layaway_Qty_TaxAmt = d.tbird_Layaway_Qty_TaxAmt,
                    tbird_Taxable = d.tbird_Taxable,
                    tbird_UnitPrice = d.tbird_UnitPrice,
                    tbird_LineTotal = d.tbird_LineTotal,
                    tbird_TaxRate = d.tbird_TaxRate,
                    tbird_TaxAmt = d.tbird_TaxAmt,

                    ItemDepartmentName = d.tbird_Item != null && d.tbird_Item.tbim_ItemCategory != null
                        ? d.tbird_Item.tbim_ItemCategory.Tbid_DepartmentName : string.Empty,
                    ItemDistributorName = d.tbird_Item != null && d.tbird_Item.tbim_Distributor != null
                        ? d.tbird_Item.tbim_Distributor.Name : string.Empty,
                    ItemLocationName = d.tbird_Item != null && d.tbird_Item.tbim_Location != null
                        ? d.tbird_Item.tbim_Location.tbld_LocationName : string.Empty,
                    ItemDisplay = d.tbird_Item != null
                        ? (d.tbird_Item.tbim_Brand + " " + d.tbird_Item.tbim_Size).Trim() : string.Empty
                })
                .FirstOrDefaultAsync();

            if (dto == null) return NotFound();
            return Ok(dto);
        }

        // PUT: api/LayawayRefundDetails/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutLayawayRefundDetails(long id, LayawayRefundDetails layawayRefundDetails)
        {
            if (id != layawayRefundDetails.Id)
            {
                return BadRequest();
            }

            _context.Entry(layawayRefundDetails).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!LayawayRefundDetailsExists(id))
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

        // POST: api/LayawayRefundDetails
        [HttpPost]
        public async Task<ActionResult<LayawayRefundDetails>> PostLayawayRefundDetails(LayawayRefundDetails layawayRefundDetails)
        {
            _context.LayawayRefundDetails.Add(layawayRefundDetails);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetLayawayRefundDetails", new { id = layawayRefundDetails.Id }, layawayRefundDetails);
        }

        // DELETE: api/LayawayRefundDetails/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteLayawayRefundDetails(long id)
        {
            var layawayRefundDetails = await _context.LayawayRefundDetails.FindAsync(id);
            if (layawayRefundDetails == null)
            {
                return NotFound();
            }

            _context.LayawayRefundDetails.Remove(layawayRefundDetails);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool LayawayRefundDetailsExists(long id)
        {
            return _context.LayawayRefundDetails.Any(e => e.Id == id);
        }
    }
}