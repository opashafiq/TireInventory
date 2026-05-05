using System.Linq;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TireInventory.Data;
using TireInventory.Models;

namespace TireInventory.Controllers
{
   // [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [Route("api/[controller]")]
    [ApiController]
    public class InvoiceRefundDetailsController : ControllerBase
    {
        private readonly ApplicationDBContext _context;

        public InvoiceRefundDetailsController(ApplicationDBContext context)
        {
            _context = context;
        }

        // GET: api/InvoiceRefundDetails
        [HttpGet]
        public async Task<ActionResult<IEnumerable<InvoiceRefundDetailsDto>>> GetInvoiceRefundDetails()
        {
            var list = await _context.InvoiceRefundDetails
                .Select(d => new InvoiceRefundDetailsDto
                {
                    Id = d.Id,
                    tbird_InvoiceRefundId = d.tbird_InvoiceRefundId,
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
                    tbird_Taxable = d.tbird_Taxable,
                    tbird_UnitPrice = d.tbird_UnitPrice,
                    tbird_LineTotal = d.tbird_LineTotal,
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

        // GET: api/InvoiceRefundDetails/5
        [HttpGet("{id}")]
        public async Task<ActionResult<InvoiceRefundDetailsDto>> GetInvoiceRefundDetails(long id)
        {
            var dto = await _context.InvoiceRefundDetails
                .Where(d => d.Id == id)
                .Select(d => new InvoiceRefundDetailsDto
                {
                    Id = d.Id,
                    tbird_InvoiceRefundId = d.tbird_InvoiceRefundId,
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
                    tbird_Taxable = d.tbird_Taxable,
                    tbird_UnitPrice = d.tbird_UnitPrice,
                    tbird_LineTotal = d.tbird_LineTotal,
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

        // PUT: api/InvoiceRefundDetails/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutInvoiceRefundDetails(long id, InvoiceRefundDetails invoiceRefundDetails)
        {
            if (id != invoiceRefundDetails.Id)
            {
                return BadRequest();
            }

            _context.Entry(invoiceRefundDetails).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!InvoiceRefundDetailsExists(id))
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

        // POST: api/InvoiceRefundDetails
        [HttpPost]
        public async Task<ActionResult<InvoiceRefundDetails>> PostInvoiceRefundDetails(InvoiceRefundDetails invoiceRefundDetails)
        {
            _context.InvoiceRefundDetails.Add(invoiceRefundDetails);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetInvoiceRefundDetails", new { id = invoiceRefundDetails.Id }, invoiceRefundDetails);
        }

        // DELETE: api/InvoiceRefundDetails/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteInvoiceRefundDetails(long id)
        {
            var invoiceRefundDetails = await _context.InvoiceRefundDetails.FindAsync(id);
            if (invoiceRefundDetails == null)
            {
                return NotFound();
            }

            _context.InvoiceRefundDetails.Remove(invoiceRefundDetails);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool InvoiceRefundDetailsExists(long id)
        {
            return _context.InvoiceRefundDetails.Any(e => e.Id == id);
        }
    }
}