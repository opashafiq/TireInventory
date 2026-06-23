using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TireInventory.Helpers;
using TireInventory.Models;

namespace TireInventory.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class AuthenticationController : ControllerBase
    {
        private readonly IJsonWebTokenService _jsonWebTokenService;
        public AuthenticationController(IJsonWebTokenService jsonWebTokenService)
        {
            this._jsonWebTokenService = jsonWebTokenService;
        }

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
    }
}
