using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using TireInventory.Models;

namespace TireInventory.Data
{
    public class ApplicationDBContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDBContext(DbContextOptions<ApplicationDBContext> dbContextOptions) : base(dbContextOptions) { }

        public DbSet<Distributors> Distributors { get; set; }
        public DbSet<Departments> Departments { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure one-to-one between TblBoBusinessInformation and TblBoLocationDetail.
            // Assumes TblBoLocationDetail.Id is both the PK and the FK that references TblBoBusinessInformation.Id.


            // If you have other generated relationships or naming conventions to preserve,
            // add them here e.g. table mappings, column names, etc.
        }
    }
}
