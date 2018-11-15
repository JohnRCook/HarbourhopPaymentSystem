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
                booking = _bookingPaymentRepository.AddBookingPayment(new Data.Models.BookingPayment { BookingId = bookingId, Amount = amount });
            }

            var molliePaymentResponse = await _paymentClient.CreatePaymentAsync(
                                            new PaymentRequest
                                            {
                                                Amount = new Amount(Currency.EUR, amount.ToString("F02", CultureInfo.InvariantCulture)),
                                                Description = $"Test Harbour Hop Payment for booking {bookingId}",
                                                RedirectUrl = _mollieOptions.RedirectUrl,
                                                WebhookUrl = _mollieOptions.WebhookUrl,
                                                Locale = locale,
                                                //Method = PaymentMethod.CreditCard | PaymentMethod.Ideal | PaymentMethod.PayPal
                                            });

            booking.TransactionId = molliePaymentResponse.Id;

            _bookingPaymentRepository.UpdateBookingPayment(booking);

            return molliePaymentResponse;
        }

        public async Task<BookingPaymentResponse> GetPaymentAsync(string paymentId)
        {
            var booking = _bookingPaymentRepository.GetBookingPayment(paymentId);
            var payment = await _paymentClient.GetPaymentAsync(paymentId);
            var bookingPayment = new BookingPaymentResponse {BookingId = booking.BookingId, PaymentStatus = payment.Status};
            return bookingPayment;
        }
    }
}
