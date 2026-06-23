using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TireInventory.Helpers;
using TireInventory.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace TireInventory.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class AuthenticationController : ControllerBase
    {
        private readonly IJsonWebTokenService _jsonWebTokenService;
        private readonly RoleManager<IdentityRole> _roleManager;
        public AuthenticationController(IJsonWebTokenService jsonWebTokenService, RoleManager<IdentityRole> roleManager)
        {
            this._jsonWebTokenService = jsonWebTokenService;
            _roleManager = roleManager;
        }


        #region Token Management
        [HttpPost("Login")]
        [AllowAnonymous]
        public async Task<ActionResult<UserDto>> Login([FromBody] LoginModel model)
        {
            try
            {
                // 1. Attempt to generate the token
                var token = await _jsonWebTokenService.GenerateTokenAsync(model.UserName, model.Password);

                // 2. Just in case it returns null instead of throwing an exception
                if (token == null || string.IsNullOrEmpty(token.Token))
                {
                    return Unauthorized(new { message = "Invalid username or password." });
                }

                // 3. Success path
                var entity = new UserDto()
                {
                    UserName = model.UserName,
                    Token = token.Token
                };

                return entity;
            }
            catch (UnauthorizedAccessException)
            {
                // 4. Catch the specific exception thrown by your service
                return Unauthorized(new { message = "Invalid username or password." });
            }
        }
        #endregion

        #region Role Management
        // 1. CREATE: Create a new role
        [HttpPost("roles")]
        public async Task<IActionResult> CreateRole([FromBody] RoleRequestDto model)
        {
            if (string.IsNullOrWhiteSpace(model.RoleName))
            {
                return BadRequest("Role name cannot be empty.");
            }

            var roleExist = await _roleManager.RoleExistsAsync(model.RoleName);
            if (roleExist)
            {
                return BadRequest("Role already exists.");
            }

            var roleResult = await _roleManager.CreateAsync(new IdentityRole(model.RoleName));

            if (roleResult.Succeeded)
            {
                return Ok(new { Message = $"Role {model.RoleName} created successfully!" });
            }

            return BadRequest(roleResult.Errors);
        }

        // 2. READ: Get all roles
        [HttpGet("roles")]
        public async Task<IActionResult> GetAllRoles()
        {
            var roles = await _roleManager.Roles.ToListAsync();
            return Ok(roles);
        }

        // 3. READ: Get a single role by ID
        [HttpGet("roles/{id}")]
        public async Task<IActionResult> GetRoleById(string id)
        {
            var role = await _roleManager.FindByIdAsync(id);
            if (role == null)
            {
                return NotFound($"Role with ID {id} not found.");
            }

            return Ok(role);
        }

        // 4. UPDATE: Update an existing role name
        [HttpPut("roles/{id}")]
        public async Task<IActionResult> UpdateRole(string id, [FromBody] RoleRequestDto model)
        {
            var role = await _roleManager.FindByIdAsync(id);
            if (role == null)
            {
                return NotFound($"Role with ID {id} not found.");
            }

            role.Name = model.RoleName;
            // The Manager automatically handles updating the NormalizedName under the hood
            var result = await _roleManager.UpdateAsync(role);

            if (result.Succeeded)
            {
                return Ok(new { Message = "Role updated successfully." });
            }

            return BadRequest(result.Errors);
        }

        // 5. DELETE: Remove a role
        [HttpDelete("roles/{id}")]
        public async Task<IActionResult> DeleteRole(string id)
        {
            var role = await _roleManager.FindByIdAsync(id);
            if (role == null)
            {
                return NotFound($"Role with ID {id} not found.");
            }

            var result = await _roleManager.DeleteAsync(role);
            if (result.Succeeded)
            {
                return Ok(new { Message = "Role deleted successfully." });
            }

            return BadRequest(result.Errors);
        }
        #endregion
    }
}
