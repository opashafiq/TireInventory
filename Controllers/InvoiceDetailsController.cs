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
    public class InvoiceDetailsController : ControllerBase
    {
        private readonly ApplicationDBContext _context;

        public InvoiceDetailsController(ApplicationDBContext context)
        {
            _context = context;
        }

        // GET: api/InvoiceDetails
        [HttpGet]
        public async Task<ActionResult<IEnumerable<InvoiceDetailsDto>>> GetInvoiceDetails()
        {
            var list = await _context.InvoiceDetails
                .Select(d => new InvoiceDetailsDto
                {
                    Id = d.Id,
                    tbid_InvoiceId = d.tbid_InvoiceId,
                    tbid_ItemId = d.tbid_ItemId,
                    tbid_ItemCategory = d.tbid_ItemCategory,
                    tbid_DepartmentName = d.tbid_DepartmentName,
                    tbid_Size = d.tbid_Size,
                    tbid_Brand = d.tbid_Brand,
                    tbid_Series = d.tbid_Series,
                    tbid_Bolt = d.tbid_Bolt,
                    tbid_HoleS = d.tbid_HoleS,
                    tbid_Zone = d.tbid_Zone,
                    tbid_DistributorId = d.tbid_DistributorId,
                    tbid_DistributorName = d.tbid_DistributorName,
                    tbid_Qty = d.tbid_Qty,
                    tbid_Taxable = d.tbid_Taxable,
                    tbid_UnitPrice = d.tbid_UnitPrice,
                    tbid_LineTotal = d.tbid_LineTotal,
                    tbid_TaxAmt = d.tbid_TaxAmt,

                    // resolved values via navigation properties
                    ItemDepartmentName = d.tbid_Item != null && d.tbid_Item.tbim_ItemCategory != null
                        ? d.tbid_Item.tbim_ItemCategory.Tbid_DepartmentName : string.Empty,
                    ItemDistributorName = d.tbid_Item != null && d.tbid_Item.tbim_Distributor != null
                        ? d.tbid_Item.tbim_Distributor.Name : string.Empty,
                    ItemLocationName = d.tbid_Item != null && d.tbid_Item.tbim_Location != null
                        ? d.tbid_Item.tbim_Location.tbld_LocationName : string.Empty,
                    ItemDisplay = d.tbid_Item != null
                        ? (d.tbid_Item.tbim_Brand + " " + d.tbid_Item.tbim_Size).Trim() : string.Empty
                })
                .ToListAsync();

            return Ok(list);
        }

        // GET: api/InvoiceDetails/5
        [HttpGet("{id}")]
        public async Task<ActionResult<InvoiceDetailsDto>> GetInvoiceDetails(long id)
        {
            var dto = await _context.InvoiceDetails
                .Where(d => d.Id == id)
                .Select(d => new InvoiceDetailsDto
                {
                    Id = d.Id,
                    tbid_InvoiceId = d.tbid_InvoiceId,
                    tbid_ItemId = d.tbid_ItemId,
                    tbid_ItemCategory = d.tbid_ItemCategory,
                    tbid_DepartmentName = d.tbid_DepartmentName,
                    tbid_Size = d.tbid_Size,
                    tbid_Brand = d.tbid_Brand,
                    tbid_Series = d.tbid_Series,
                    tbid_Bolt = d.tbid_Bolt,
                    tbid_HoleS = d.tbid_HoleS,
                    tbid_Zone = d.tbid_Zone,
                    tbid_DistributorId = d.tbid_DistributorId,
                    tbid_DistributorName = d.tbid_DistributorName,
                    tbid_Qty = d.tbid_Qty,
                    tbid_Taxable = d.tbid_Taxable,
                    tbid_UnitPrice = d.tbid_UnitPrice,
                    tbid_LineTotal = d.tbid_LineTotal,
                    tbid_TaxAmt = d.tbid_TaxAmt,

                    ItemDepartmentName = d.tbid_Item != null && d.tbid_Item.tbim_ItemCategory != null
                        ? d.tbid_Item.tbim_ItemCategory.Tbid_DepartmentName : string.Empty,
                    ItemDistributorName = d.tbid_Item != null && d.tbid_Item.tbim_Distributor != null
                        ? d.tbid_Item.tbim_Distributor.Name : string.Empty,
                    ItemLocationName = d.tbid_Item != null && d.tbid_Item.tbim_Location != null
                        ? d.tbid_Item.tbim_Location.tbld_LocationName : string.Empty,
                    ItemDisplay = d.tbid_Item != null
                        ? (d.tbid_Item.tbim_Brand + " " + d.tbid_Item.tbim_Size).Trim() : string.Empty
                })
                .FirstOrDefaultAsync();

            if (dto == null) return NotFound();
            return Ok(dto);
        }

        // PUT: api/InvoiceDetails/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutInvoiceDetails(long id, InvoiceDetails invoiceDetails)
        {
            if (id != invoiceDetails.Id)
            {
                return BadRequest();
            }

            _context.Entry(invoiceDetails).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!InvoiceDetailsExists(id))
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

        // POST: api/InvoiceDetails
        [HttpPost]
        public async Task<ActionResult<InvoiceDetails>> PostInvoiceDetails(InvoiceDetails invoiceDetails)
        {
            _context.InvoiceDetails.Add(invoiceDetails);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetInvoiceDetails", new { id = invoiceDetails.Id }, invoiceDetails);
        }

        // DELETE: api/InvoiceDetails/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteInvoiceDetails(long id)
        {
            var invoiceDetails = await _context.InvoiceDetails.FindAsync(id);
            if (invoiceDetails == null)
            {
                return NotFound();
            }

            _context.InvoiceDetails.Remove(invoiceDetails);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool InvoiceDetailsExists(long id)
        {
            return _context.InvoiceDetails.Any(e => e.Id == id);
        }
    }
}