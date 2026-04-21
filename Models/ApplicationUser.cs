using Microsoft.AspNetCore.Identity;

namespace TireInventory.Models
{
    public class ApplicationUser : IdentityUser
    {
        public ApplicationUser ShallowCopy()
        {
            return (ApplicationUser)MemberwiseClone();
        }
    }
}
