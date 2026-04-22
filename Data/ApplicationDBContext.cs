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

        // Existing DbSets
        public DbSet<CompanyInfo> CompanyInfos { get; set; }
        public DbSet<ItemMaster> ItemMasters { get; set; }
        public DbSet<LocationDetails> LocationDetails { get; set; }
        public DbSet<TaxId> TaxIds { get; set; }
        public DbSet<ExpenseHead> ExpenseHeads { get; set; }
        public DbSet<PaymentNames> PaymentNames { get; set; }

        // Added DbSets for new controllers
        public DbSet<RefundMethodNames> RefundMethodNames { get; set; }
        public DbSet<TaxRateModified> TaxRateModifieds { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // preserve any custom model configuration here (one-to-one, table mappings, etc.)
        }
    }
}
