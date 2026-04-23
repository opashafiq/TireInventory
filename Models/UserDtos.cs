using System.Collections.Generic;

namespace TireInventory.Models
{
    public class CreateUserDto
    {
        public string UserName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string? Role { get; set; }
    }

    public class UpdateUserDto
    {
        public string UserName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? Password { get; set; } // optional
        public string? Role { get; set; }
    }

    public class IdentityUserDto
    {
        public string Id { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        // Use a concrete collection type and initialize to avoid reflection/serialization issues
        public List<string> Roles { get; set; } = new List<string>();
    }
}