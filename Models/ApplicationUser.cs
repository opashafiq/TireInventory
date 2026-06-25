using Microsoft.AspNetCore.Identity;

namespace TireInventory.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public bool IsActive { get; set; }
        public long LocationId { get; set; }

        // Custom property
        //public string Department { get; set; }
        public ApplicationUser ShallowCopy()
        {
            return (ApplicationUser)MemberwiseClone();
        }
    }
}
