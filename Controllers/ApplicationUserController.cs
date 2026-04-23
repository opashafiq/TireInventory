using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TireInventory.Data;
using TireInventory.Models;

namespace TireInventory.Controllers
{
    //[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin")]
    [Route("api/[controller]")]
    [ApiController]
    public class ApplicationUserController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ApplicationDBContext _context;

        public ApplicationUserController(
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager,
            ApplicationDBContext context)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _context = context;
        }

        

        // GET: api/ApplicationUser
        [HttpGet]
        public async Task<ActionResult<IEnumerable<IdentityUserDto>>> GetUsers()
        {
            var users = await _userManager.Users.ToListAsync();
            var result = new List<IdentityUserDto>();

            foreach (var u in users)
            {
                var roles = (await _userManager.GetRolesAsync(u)).ToList();
                result.Add(new IdentityUserDto
                {
                    Id = u.Id,
                    UserName = u.UserName ?? string.Empty,
                    Email = u.Email ?? string.Empty,
                    Roles = roles
                });
            }

            return Ok(result);
        }



        // GET: api/ApplicationUser/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<IdentityUserDto>> GetUser(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            var roles = (await _userManager.GetRolesAsync(user)).ToList();

            return Ok(new IdentityUserDto
            {
                Id = user.Id,
                UserName = user.UserName,
                Email = user.Email,
                Roles = roles
            });
        }

        // POST: api/ApplicationUser
        [HttpPost]
        public async Task<ActionResult<IdentityUserDto>> CreateUser(CreateUserDto dto)
        {
            if (dto == null) return BadRequest();

            var user = new ApplicationUser
            {
                UserName = dto.UserName,
                Email = dto.Email
            };

            var createResult = await _userManager.CreateAsync(user, dto.Password);
            if (!createResult.Succeeded)
            {
                return BadRequest(createResult.Errors);
            }

            if (!string.IsNullOrWhiteSpace(dto.Role))
            {
                if (!await _roleManager.RoleExistsAsync(dto.Role))
                {
                    var roleResult = await _roleManager.CreateAsync(new IdentityRole(dto.Role));
                    if (!roleResult.Succeeded)
                    {
                        // rollback user creation on role creation failure
                        await _userManager.DeleteAsync(user);
                        return BadRequest(roleResult.Errors);
                    }
                }

                var addRoleResult = await _userManager.AddToRoleAsync(user, dto.Role);
                if (!addRoleResult.Succeeded)
                {
                    await _userManager.DeleteAsync(user);
                    return BadRequest(addRoleResult.Errors);
                }
            }

            var roles = (await _userManager.GetRolesAsync(user)).ToList();

            var created = new IdentityUserDto
            {
                Id = user.Id,
                UserName = user.UserName,
                Email = user.Email,
                Roles = roles
            };

            return CreatedAtAction(nameof(GetUser), new { id = user.Id }, created);
        }

        // PUT: api/ApplicationUser/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUser(string id, UpdateUserDto dto)
        {
            if (dto == null) return BadRequest();

            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            user.UserName = dto.UserName;
            user.Email = dto.Email;

            var updateResult = await _userManager.UpdateAsync(user);
            if (!updateResult.Succeeded) return BadRequest(updateResult.Errors);

            if (!string.IsNullOrWhiteSpace(dto.Password))
            {
                var token = await _userManager.GeneratePasswordResetTokenAsync(user);
                var passResult = await _userManager.ResetPasswordAsync(user, token, dto.Password);
                if (!passResult.Succeeded) return BadRequest(passResult.Errors);
            }

            if (!string.IsNullOrWhiteSpace(dto.Role))
            {
                if (!await _roleManager.RoleExistsAsync(dto.Role))
                {
                    var roleResult = await _roleManager.CreateAsync(new IdentityRole(dto.Role));
                    if (!roleResult.Succeeded) return BadRequest(roleResult.Errors);
                }

                var currentRoles = await _userManager.GetRolesAsync(user);
                if (!currentRoles.Contains(dto.Role))
                {
                    var removeResult = await _userManager.RemoveFromRolesAsync(user, currentRoles);
                    if (!removeResult.Succeeded) return BadRequest(removeResult.Errors);

                    var addResult = await _userManager.AddToRoleAsync(user, dto.Role);
                    if (!addResult.Succeeded) return BadRequest(addResult.Errors);
                }
            }

            return NoContent();
        }

        // DELETE: api/ApplicationUser/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            var deleteResult = await _userManager.DeleteAsync(user);
            if (!deleteResult.Succeeded) return BadRequest(deleteResult.Errors);

            return NoContent();
        }
    }
}