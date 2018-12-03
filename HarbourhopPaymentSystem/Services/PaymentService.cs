using HarbourhopPaymentSystem.Data.Repositories;
using HarbourhopPaymentSystem.Models;
using Microsoft.Extensions.Options;
using Mollie.Api.Client;
using Mollie.Api.Models;
using Mollie.Api.Models.Payment.Request;
using Mollie.Api.Models.Payment.Response;
using System.Threading.Tasks;
using System.Globalization;
using HarbourhopPaymentSystem.Responses;
using Mollie.Api.Models.Payment;
using HarbourhopPaymentSystem.Data.Models;
using System;

namespace HarbourhopPaymentSystem.Services
{
    public class PaymentService
    {
        private readonly BookingPaymentRepository _bookingPaymentRepository;
        private readonly MollieOptions _mollieOptions;
        private readonly PaymentClient _paymentClient;


        public PaymentService(BookingPaymentRepository bookingPaymentRepository, IOptionsSnapshot<MollieOptions> mollieOptions)
        {
            _bookingPaymentRepository = bookingPaymentRepository;
            _mollieOptions = mollieOptions.Value;
            _paymentClient = new PaymentClient(_mollieOptions.MollieApiKey);
        }

        public BookingPayment GetBookingPayment(int bookingId)
        {
            try
            {
                var booking = _bookingPaymentRepository.GetBookingPayment(bookingId);
                return booking;
            }
            catch (Exception)
            {
                // Do nothing.
            }
            return null;
        }

        public async Task ValidateBookingPayment(int bookingId, double amountToPay)
        {
            var booking = _bookingPaymentRepository.GetBookingPayment(bookingId);

            if (!string.IsNullOrEmpty(booking?.TransactionId))
            {
                var payment = await _paymentClient.GetPaymentAsync(booking.TransactionId);
                var amount = new Amount(Currency.EUR, amountToPay.ToString("F02", CultureInfo.InvariantCulture));
                if (payment.Status == PaymentStatus.Paid && payment.Amount == amount)
                {
                    throw new PaymentAlreadyExistsException();
                }
            }
        }

        public async Task<PaymentResponse> CreatePayment(int bookingId, double amount, string locale)
        {
            var booking = _bookingPaymentRepository.GetBookingPayment(bookingId);

            if(booking == null)
            {
                booking = _bookingPaymentRepository.AddBookingPayment(new BookingPayment { BookingId = bookingId, Amount = amount });
            }

            var molliePaymentResponse = await _paymentClient.CreatePaymentAsync(
                                            new PaymentRequest
                                            {
                                                Amount = new Amount(Currency.EUR, amount.ToString("F02", CultureInfo.InvariantCulture)),
                                                Description = $"Payment for booking {bookingId}",
                                                RedirectUrl = $"{_mollieOptions.RedirectUrl}/{bookingId}",
                                                WebhookUrl = _mollieOptions.WebhookUrl,
                                                Metadata = bookingId.ToString()
                                            });

            booking.TransactionId = molliePaymentResponse.Id;

            _bookingPaymentRepository.UpdateBookingPayment(booking);

            return molliePaymentResponse;
        }

        public void SetBookingPaymentStatus(int bookingId, bool success)
        {
            try
            {
                var bookingPayment = GetBookingPayment(bookingId);

                if (bookingPayment != null)
                {
                    bookingPayment.Success = success;
                    _bookingPaymentRepository.UpdateBookingPayment(bookingPayment);
                }
            }
            catch (Exception)
            {
                // Do nothing. Just go on!
            }
        }

        public async Task<BookingPaymentResponse> GetPaymentAsync(string paymentId)
        {
            var payment = await _paymentClient.GetPaymentAsync(paymentId);
            var bookingId = int.Parse(payment.Metadata);
            var bookingPayment = new BookingPaymentResponse {BookingId = bookingId, PaymentStatus = payment.Status};
            return bookingPayment;
        }
    }
}
