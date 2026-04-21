namespace TireInventory.Models
{
    public class TokenDto
    {
        public TokenDto(string token, DateTime expiresIn, string tokenType)
        {
            Token = token;
            ExpiresIn = expiresIn;
            TokenType = tokenType;
        }

        public string Token { get; set; } = default!;

        public DateTime ExpiresIn { get; set; }

        public string TokenType { get; set; } = default!;
    }

}
