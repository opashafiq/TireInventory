using TireInventory.Models;

namespace TireInventory.Helpers
{
    public interface IJsonWebTokenService
    {
        Task<TokenDto> GenerateTokenAsync(string userName, string password);
    }
}
