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
    public class LocationDetailsController : ControllerBase
    {
        private readonly ApplicationDBContext _context;

        public LocationDetailsController(ApplicationDBContext context)
        {
            _context = context;
        }

        // GET: api/LocationDetails
        [HttpGet]
        public async Task<ActionResult<IEnumerable<LocationDetails>>> GetLocationDetails()
        {
            return await _context.LocationDetails.ToListAsync();
        }

        // GET: api/LocationDetails/5
        [HttpGet("{id}")]
        public async Task<ActionResult<LocationDetails>> GetLocationDetails(long id)
        {
            var locationDetails = await _context.LocationDetails.FindAsync(id);

            if (locationDetails == null)
            {
                return NotFound();
            }

            return locationDetails;
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