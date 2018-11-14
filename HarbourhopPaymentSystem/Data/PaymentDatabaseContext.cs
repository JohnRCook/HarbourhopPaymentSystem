using HarbourhopPaymentSystem.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace HarbourhopPaymentSystem.Data
{
    public class PaymentDatabaseContext: DbContext
    {
        public PaymentDatabaseContext(DbContextOptions<PaymentDatabaseContext> options) : base(options)
        {
        }

        public DbSet<BookingPayment> BookingPayments { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<BookingPayment>().HasIndex(x => new { x.BookingId }).IsUnique();
        }

    }
}
