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
    public class LocationDetailsController : ControllerBase
    {
        private readonly ApplicationDBContext _context;

        public LocationDetailsController(ApplicationDBContext context)
        {
            _context = context;
        }

        // GET: api/LocationDetails
        [HttpGet]
        public async Task<ActionResult<IEnumerable<LocationDetailsDto>>> GetLocationDetails()
        {
            var list = await _context.LocationDetails
                .Select(ld => new LocationDetailsDto
                {
                    Id = ld.Id,
                    tbld_LocationName = ld.tbld_LocationName,
                    CompanyInfoId = ld.CompanyInfoId,
                    tbld_Address1 = ld.tbld_Address1,
                    tbld_Address2 = ld.tbld_Address2,
                    tbld_City = ld.tbld_City,
                    tbld_State = ld.tbld_State,
                    tbld_ZipCode = ld.tbld_ZipCode,
                    tbld_Phone = ld.tbld_Phone,
                    tbld_Fax = ld.tbld_Fax,
                    tbld_Email = ld.tbld_Email,
                    CompanyName = ld.CompanyInfo != null ? ld.CompanyInfo.tbbiBusinessName : string.Empty
                })
                .ToListAsync();

            return Ok(list);
        }

        // GET: api/LocationDetails/5
        [HttpGet("{id}")]
        public async Task<ActionResult<LocationDetailsDto>> GetLocationDetails(long id)
        {
            var dto = await _context.LocationDetails
                .Where(ld => ld.Id == id)
                .Select(ld => new LocationDetailsDto
                {
                    Id = ld.Id,
                    tbld_LocationName = ld.tbld_LocationName,
                    CompanyInfoId = ld.CompanyInfoId,
                    tbld_Address1 = ld.tbld_Address1,
                    tbld_Address2 = ld.tbld_Address2,
                    tbld_City = ld.tbld_City,
                    tbld_State = ld.tbld_State,
                    tbld_ZipCode = ld.tbld_ZipCode,
                    tbld_Phone = ld.tbld_Phone,
                    tbld_Fax = ld.tbld_Fax,
                    tbld_Email = ld.tbld_Email,
                    CompanyName = ld.CompanyInfo != null ? ld.CompanyInfo.tbbiBusinessName : string.Empty
                })
                .FirstOrDefaultAsync();

            if (dto == null) return NotFound();
            return Ok(dto);
        }

        // PUT: api/LocationDetails/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutLocationDetails(long id, LocationDetails locationDetails)
        {
            if (id != locationDetails.Id)
            {
                return BadRequest();
            }

            _context.Entry(locationDetails).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!LocationDetailsExists(id))
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

        // POST: api/LocationDetails
        [HttpPost]
        public async Task<ActionResult<LocationDetails>> PostLocationDetails(LocationDetails locationDetails)
        {
            _context.LocationDetails.Add(locationDetails);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetLocationDetails", new { id = locationDetails.Id }, locationDetails);
        }

        // DELETE: api/LocationDetails/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteLocationDetails(long id)
        {
            var locationDetails = await _context.LocationDetails.FindAsync(id);
            if (locationDetails == null)
            {
                return NotFound();
            }

            _context.LocationDetails.Remove(locationDetails);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool LocationDetailsExists(long id)
        {
            return _context.LocationDetails.Any(e => e.Id == id);
        }
    }
}