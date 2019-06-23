using Microsoft.EntityFrameworkCore;
using Model;

namespace Db
{
    public class LoanContext : DbContext
    {
        public DbSet<Thing> Things { get; set; }
        public DbSet<Loan> Loans { get; set; }
        public DbSet<Lender> Lenders { get; set; }
        public DbSet<LenderContact> LenderContacts { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer(@"Server=(localdb)\mssqllocaldb;Database=EFCoreTest;Trusted_Connection=True;ConnectRetryCount=0");
            base.OnConfiguring(optionsBuilder);
        }

    }

}