using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;


namespace HarbourhopPaymentSystem.Data
{
    public class DatabaseInitializer
    {
        private readonly PaymentDatabaseContext _context;

        public DatabaseInitializer(PaymentDatabaseContext context)
        {
            _context = context;
        }

        public void Initialize()
        {
            List<string> pendingMigrations = _context.Database.GetPendingMigrations().ToList();

            if (!pendingMigrations.Any())
            {
                return;
            }

            _context.Database.Migrate();
        }
    }
}
