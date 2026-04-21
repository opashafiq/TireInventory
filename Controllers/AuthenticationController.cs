using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TireInventory.Helpers;
using TireInventory.Models;

namespace TireInventory.Controllers
{
    public class AuthenticationController
    {
        private readonly IJsonWebTokenService _jsonWebTokenService;
        public AuthenticationController(IJsonWebTokenService jsonWebTokenService)
        {
            this._jsonWebTokenService = jsonWebTokenService;
        }

        [HttpPost("Login")]
        [AllowAnonymous]
        public async Task<UserDto> Login([FromBody] LoginModel model)
        {
            string UserName = model.UserName;
            var token = await _jsonWebTokenService.GenerateTokenAsync(model.UserName, model.Password);
            var entity = new UserDto()
            {
                UserName = UserName,
                Token = token.Token
            };
            return entity;
        }
    }
}
