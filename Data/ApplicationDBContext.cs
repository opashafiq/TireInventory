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

        // Previously added DbSets...
        public DbSet<CompanyInfo> CompanyInfos { get; set; }
        public DbSet<ItemMaster> ItemMasters { get; set; }
        public DbSet<LocationDetails> LocationDetails { get; set; }
        public DbSet<TaxId> TaxIds { get; set; }
        public DbSet<ExpenseHead> ExpenseHeads { get; set; }
        public DbSet<PaymentNames> PaymentNames { get; set; }
        public DbSet<RefundMethodNames> RefundMethodNames { get; set; }
        public DbSet<TaxRateModified> TaxRateModifieds { get; set; }

        // Invoice-related DbSets
        public DbSet<InvoiceMaster> InvoiceMasters { get; set; }
        public DbSet<InvoiceDetails> InvoiceDetails { get; set; }
        public DbSet<InvoicePayments> InvoicePayments { get; set; }
        public DbSet<InvoiceRefundMaster> InvoiceRefundMasters { get; set; }
        public DbSet<InvoiceRefundDetails> InvoiceRefundDetails { get; set; }
        public DbSet<InvoiceRefundPayments> InvoiceRefundPayments { get; set; }


        // Layaway-related DbSets
        public DbSet<LayawayMaster> LayawayMasters { get; set; }
        public DbSet<LayawayDetails> LayawayDetails { get; set; }
        public DbSet<LayawayPayments> LayawayPayments { get; set; }
        public DbSet<LayawayRefundMaster> LayawayRefundMasters { get; set; }
        public DbSet<LayawayRefundDetails> LayawayRefundDetails { get; set; }
        public DbSet<LayawayRefundPayments> LayawayRefundPayments { get; set; }
        public DbSet<DailyExpense> DailyExpenses { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // preserve any custom model configuration here (one-to-one, table mappings, etc.)
            modelBuilder.Entity<ItemMaster>(entity =>
            {
                // The string value does not need to match your actual database trigger name
                entity.ToTable("tbl_ItemMaster", tb => tb.HasTrigger("tg_ItemMaster_Insert"));
            });              
            
            modelBuilder.Entity<InvoicePayments>(entity =>
            {
                // The string value does not need to match your actual database trigger name
                entity.ToTable("tbl_Invoice_Payment", tb => tb.HasTrigger("tg_Invoice_Payment_Insert"));
            });              
            
            modelBuilder.Entity<LayawayPayments>(entity =>
            {
                // The string value does not need to match your actual database trigger name
                entity.ToTable("tbl_Layaway_Payment", tb => tb.HasTrigger("tg_tbl_Layaway_Payment_Insert"));
            });  
            

        }
    }
}
