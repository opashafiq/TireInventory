using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using TireInventory.Models;

namespace TireInventory.Helpers
{
    public class JsonWebTokenService : IJsonWebTokenService
    {
        SignInManager<ApplicationUser> signInManager;

        public JsonWebTokenService(SignInManager<ApplicationUser> signInManager)
        {
            this.signInManager = signInManager;
        }

        public async Task<TokenDto> GenerateTokenAsync(string userName, string password)
        {
            var signInResult = await signInManager.PasswordSignInAsync(userName, password, false, false);
            if (signInResult.Succeeded)
            {
                var signingkey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("this-is-signing-key this-is-signing-key this-is-signing-key"));
                var signingCredentials = new SigningCredentials(signingkey, SecurityAlgorithms.HmacSha256);
                var jwt = new JwtSecurityToken(signingCredentials: signingCredentials, expires: DateTime.Now.AddMinutes(30));
                //string token = new JwtSecurityTokenHandler().WriteToken(jwt);
                return new TokenDto(new JwtSecurityTokenHandler().WriteToken(jwt),
                jwt.ValidTo, "Bearer");
            }
            //else if (signInResult.Succeed)
            else throw new UnauthorizedAccessException();
        }
    }
}
