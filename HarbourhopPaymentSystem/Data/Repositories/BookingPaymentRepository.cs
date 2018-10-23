using HarbourhopPaymentSystem.Data.Models;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;

namespace HarbourhopPaymentSystem.Data.Repositories
{
    public class BookingPaymentRepository : BaseRepository
    {

        public BookingPaymentRepository(PaymentDatabaseContext context, ILogger<BookingPaymentRepository> logger) : base(context, logger)
        {
        }

        public BookingPayment GetBookingPayment(int bookingId)
        {
            try
            {
                return Context.BookingPayments.SingleOrDefault(x => x.BookingId == bookingId);
            }
            catch (Exception ex)
            {
                Logger.LogError($"Failed to get the booking payment with ID {bookingId} to the database.", ex);
                throw;
            }
        }

        public void AddBookingPayment(BookingPayment bookingPayment)
        {
            try
            {
                Context.BookingPayments.Add(bookingPayment);
                Context.SaveChanges();
            }
            catch (Exception ex)
            {
                Logger.LogError($"Failed to add the booking payment with ID {bookingPayment.BookingId} to the database.", ex);
                throw;
            }
        }

        public void UpdateBookingPayment(BookingPayment bookingPayment)
        {
            try
            {
                //var currentBookingPayment = Context.BookingPayments.AsNoTracking().SingleOrDefault(x => x.Id == controleMethode.Id);
                var currentBookingPayment = Context.BookingPayments.SingleOrDefault(x => x.BookingId == bookingPayment.BookingId);
                if (currentBookingPayment != null)
                {
                    Context.BookingPayments.Update(bookingPayment);
                    Context.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                Logger.LogError($"Cannot update the booking payment with ID {bookingPayment.BookingId}.", ex);
                throw;
            }
        }
    }
}
