using System.Collections.Generic;
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
    public class DepartmentsController : ControllerBase
    {
        private readonly ApplicationDBContext _context;

        public DepartmentsController(ApplicationDBContext context)
        {
            _context = context;
        }

        // GET: api/TblBoItemDepartment
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Department>>> GetTblBoItemDepartment()
        {
            return await _context.TblBoItemDepartments.ToListAsync();
        }

        // GET: api/TblBoItemDepartment/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Department>> GetTblBoItemDepartment(long id)
        {
            var tblBoItemDepartment = await _context.TblBoItemDepartments.FindAsync(id);

            if (tblBoItemDepartment == null)
            {
                return NotFound();
            }

            return tblBoItemDepartment;
        }

        // PUT: api/TblBoItemDepartment/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutTblBoItemDepartment(long id, Department tblBoItemDepartment)
        {
            if (id != tblBoItemDepartment.Id)
            {
                return BadRequest();
            }

            _context.Entry(tblBoItemDepartment).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!TblBoItemDepartmentExists(id))
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

        // POST: api/TblBoItemDepartment
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<Department>> PostTblBoItemDepartment(Department tblBoItemDepartment)
        {
            _context.TblBoItemDepartments.Add(tblBoItemDepartment);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetTblBoItemDepartment", new { id = tblBoItemDepartment.Id }, tblBoItemDepartment);
        }

        // DELETE: api/TblBoItemDepartment/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTblBoItemDepartment(long id)
        {
            var tblBoItemDepartment = await _context.TblBoItemDepartments.FindAsync(id);
            if (tblBoItemDepartment == null)
            {
                return NotFound();
            }

            _context.TblBoItemDepartments.Remove(tblBoItemDepartment);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool TblBoItemDepartmentExists(long id)
        {
            return _context.TblBoItemDepartments.Any(e => e.Id == id);
        }
    }
}