using System;
using System.Linq;
using System.Threading.Tasks;
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
    public class LayawayMasterController : ControllerBase
    {
        private readonly ApplicationDBContext _context;

        public LayawayMasterController(ApplicationDBContext context)
        {
            _context = context;
        }

        // GET: api/LayawayMaster
        [HttpGet]
        public async Task<ActionResult<IEnumerable<LayawayMasterDto>>> GetLayawayMasters()
        {
            var list = await _context.LayawayMasters
                .Select(m => new LayawayMasterDto
                {
                    Id = m.Id,
                    tbim_Phone = m.tbim_Phone,
                    tbim_InvDate = m.tbim_InvDate,
                    tbim_Name = m.tbim_Name,
                    tbim_TaxId = m.tbim_TaxId,
                    tbim_VehicleMake = m.tbim_VehicleMake,
                    tbim_Model = m.tbim_Model,
                    tbim_Year = m.tbim_Year,
                    tbim_SubTotal = m.tbim_SubTotal,
                    tbim_SaleTax = m.tbim_SaleTax,
                    tbim_Total = m.tbim_Total,
                    UserName = m.UserName,
                    SetDate = m.SetDate,
                    LocationName = m.tbim_Location != null ? m.tbim_Location.tbld_LocationName : string.Empty,
                    TaxCompanyName = m.tbim_Tax != null ? m.tbim_Tax.tbti_ComName : string.Empty
                })
                .ToListAsync();

            return Ok(list);
        }

        // GET: api/LayawayMaster/5
        [HttpGet("{id}")]
        public async Task<ActionResult<LayawayMasterDto>> GetLayawayMaster(long id)
        {
            var dto = await _context.LayawayMasters
                .Where(m => m.Id == id)
                .Select(m => new LayawayMasterDto
                {
                    Id = m.Id,
                    tbim_Phone = m.tbim_Phone,
                    tbim_InvDate = m.tbim_InvDate,
                    tbim_Name = m.tbim_Name,
                    tbim_TaxId = m.tbim_TaxId,
                    tbim_VehicleMake = m.tbim_VehicleMake,
                    tbim_Model = m.tbim_Model,
                    tbim_Year = m.tbim_Year,
                    tbim_SubTotal = m.tbim_SubTotal,
                    tbim_SaleTax = m.tbim_SaleTax,
                    tbim_Total = m.tbim_Total,
                    UserName = m.UserName,
                    SetDate = m.SetDate,
                    LocationName = m.tbim_Location != null ? m.tbim_Location.tbld_LocationName : string.Empty,
                    TaxCompanyName = m.tbim_Tax != null ? m.tbim_Tax.tbti_ComName : string.Empty
                })
                .FirstOrDefaultAsync();

            if (dto == null) return NotFound();
            return Ok(dto);
        }


        // GET: api/LayawayMaster/{id}/details
        [HttpGet("{id}/details")]
        public async Task<ActionResult<IEnumerable<LayawayDetailsDto>>> GetLayawayDetailsByLayaway(long id)
        {
            var list = await _context.LayawayDetails
                .Where(d => d.tbid_InvoiceId == id)
                .Select(d => new LayawayDetailsDto
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
                .ToListAsync();

            return Ok(list);
        }

        // GET: api/LayawayMaster/{id}/payments
        [HttpGet("{id}/payments")]
        public async Task<ActionResult<IEnumerable<LayawayPaymentsDto>>> GetLayawayPaymentsByLayaway(long id)
        {
            var list = await (from p in _context.LayawayPayments
                              join pay in _context.PaymentNames
                                  on p.tbip_PaymentId equals pay.Id into gj
                              from pay in gj.DefaultIfEmpty()
                              where p.tbip_InvoiceId == id
                              select new LayawayPaymentsDto
                              {
                                  Id = p.Id,
                                  tbip_InvoiceId = p.tbip_InvoiceId,
                                  tbip_PaymentId = p.tbip_PaymentId,
                                  tbip_PayAmt = p.tbip_PayAmt,
                                  tbip_Date = p.tbip_Date,
                                  tbip_PaymentType = p.tbip_PaymentType,
                                  PaymentName = pay != null ? pay.tbpn_PaymentName : string.Empty
                              })
                             .ToListAsync();

            return Ok(list);
        }

        // PUT: api/LayawayMaster/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutLayawayMaster(long id, LayawayMaster layawayMaster)
        {
            if (id != layawayMaster.Id)
            {
                return BadRequest();
            }

            _context.Entry(layawayMaster).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!LayawayMasterExists(id))
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

        // POST: api/LayawayMaster
        [HttpPost]
        public async Task<ActionResult<LayawayMaster>> PostLayawayMaster(LayawayMaster layawayMaster)
        {
            _context.LayawayMasters.Add(layawayMaster);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetLayawayMaster", new { id = layawayMaster.Id }, layawayMaster);
        }

        // POST: api/LayawayMaster/CreateLayaway
        // Accepts a master, a list of details and a list of payments.
        // Inserts master first, then inserts details and payments with FK set to master.Id
        [HttpPost("CreateLayaway")]
        public async Task<ActionResult<LayawayMaster>> CreateLayaway(CreateLayawayDto dto)
        {
            if (dto == null) return BadRequest();

            var layawayMaster = dto.Layaway ?? new LayawayMaster();

            // Insert master to obtain generated Id
            _context.LayawayMasters.Add(layawayMaster);
            await _context.SaveChangesAsync();

            // DETAILS: set FK and enrich from ItemMaster/Departments/Distributors as before
            var details = dto.LayawayDetails ?? new List<LayawayDetails>();
            foreach (var d in details)
            {
                d.tbid_InvoiceId = layawayMaster.Id;
                d.Id = 0;
                d.tbid_Invoice = null;

                if (d.tbid_ItemId.HasValue)
                {
                    var itemMaster = await _context.ItemMasters.FindAsync(d.tbid_ItemId.Value);
                    if (itemMaster != null)
                    {
                        try
                        {
                            d.tbid_ItemCategory = Convert.ToInt32(itemMaster.tbim_ItemCategoryId);
                        }
                        catch
                        {
                            d.tbid_ItemCategory = null;
                        }

                        var dept = await _context.Departments.FindAsync(itemMaster.tbim_ItemCategoryId);
                        d.tbid_DepartmentName = dept?.Tbid_DepartmentName;

                        d.tbid_Size = itemMaster.tbim_Size;
                        d.tbid_Brand = itemMaster.tbim_Brand;
                        d.tbid_Series = itemMaster.tbim_Series;
                        d.tbid_Bolt = itemMaster.tbim_Bolt;
                        d.tbid_HoleS = itemMaster.tbim_HoleS;
                        d.tbid_Zone = itemMaster.tbim_Zone;
                        d.tbid_DistributorId = itemMaster.tbim_DistributorId;

                        if (itemMaster.tbim_DistributorId.HasValue)
                        {
                            var distributor = await _context.Distributors.FindAsync(itemMaster.tbim_DistributorId.Value);
                            d.tbid_DistributorName = distributor?.Name;
                        }
                    }
                }
            }

            if (details.Count > 0)
            {
                _context.LayawayDetails.AddRange(details);
                await _context.SaveChangesAsync();
            }

            // PAYMENTS: set FK to created master Id and insert
            var payments = dto.LayawayPayments ?? new List<LayawayPayments>();
            foreach (var p in payments)
            {
                p.tbip_InvoiceId = layawayMaster.Id;
                p.Id = 0;
                p.tbip_Invoice = null;
            }

            if (payments.Count > 0)
            {
                _context.LayawayPayments.AddRange(payments);
                await _context.SaveChangesAsync();
            }

            // Return created layaway master including its details and payments
            var created = await _context.LayawayMasters
                .Include(m => m.tbl_Layaway_Details)
                .Include(m => m.tbl_Layaway_Payments)
                .FirstOrDefaultAsync(m => m.Id == layawayMaster.Id);

            return CreatedAtAction(nameof(GetLayawayMaster), new { id = layawayMaster.Id }, created ?? layawayMaster);
        }

        // DELETE: api/LayawayMaster/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteLayawayMaster(long id)
        {
            var layawayMaster = await _context.LayawayMasters.FindAsync(id);
            if (layawayMaster == null)
            {
                return NotFound();
            }

            _context.LayawayMasters.Remove(layawayMaster);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool LayawayMasterExists(long id)
        {
            return _context.LayawayMasters.Any(e => e.Id == id);
        }
    }
}