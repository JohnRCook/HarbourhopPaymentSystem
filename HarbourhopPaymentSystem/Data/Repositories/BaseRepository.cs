using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace HarbourhopPaymentSystem.Data.Repositories
{
    public class BaseRepository
    {
        public PaymentDatabaseContext Context { get; }
        public ILogger<BaseRepository> Logger { get; }

        protected BaseRepository(PaymentDatabaseContext context, ILogger<BaseRepository> logger)
        {
            Context = context;
            Logger = logger;
            Context.Database.SetCommandTimeout(3600);
        }

        public bool SaveAll()
        {
            try
            {
                return Context.SaveChanges() >= 0;
            }
            catch (Exception ex)
            {
                Logger?.LogError("Could not save all changes to the database", ex);
                return false;
            }
        }

        private bool _disposed;

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    Context.Dispose();
                }
            }
            _disposed = true;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}