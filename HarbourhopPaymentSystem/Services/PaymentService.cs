using HarbourhopPaymentSystem.Data.Repositories;
using HarbourhopPaymentSystem.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Mollie.Api.Client;
using Mollie.Api.Models;
using Mollie.Api.Models.Payment.Request;
using Mollie.Api.Models.Payment.Response;
using Mollie.Api.Models.Payment;
using System.Threading.Tasks;
using System.Globalization;

namespace HarbourhopPaymentSystem.Services
{
    public class PaymentService
    {
        private readonly BookingPaymentRepository _bookingPaymentRepository;
        private readonly ILogger<PaymentService> _logger;
        private readonly MollieOptions _mollieOptions;
        private readonly PaymentClient _paymentClient;


        public PaymentService(BookingPaymentRepository bookingPaymentRepository, IOptionsSnapshot<MollieOptions> mollieOptions, ILogger<PaymentService> logger)
        {
            _bookingPaymentRepository = bookingPaymentRepository;
            _logger = logger;
            _mollieOptions = mollieOptions.Value;
            _paymentClient = new PaymentClient(_mollieOptions.MollieApiKey);
        }

        public async Task<PaymentResponse> CreatePayment(int bookingId, double amount, string locale)
        {
            //TODO: if booking exists? what is the scenario? 

            var booking = _bookingPaymentRepository.GetBookingPayment(bookingId);

            if (!string.IsNullOrEmpty(booking?.TransactionId))
            {
                throw new BookingAlreadyExistsException();
            }

            if (booking == null)
            {
                booking = _bookingPaymentRepository.AddBookingPayment(new Data.Models.BookingPayment { BookingId = bookingId, Amount = amount });
            }

            var molliePaymentResponse = await _paymentClient.CreatePaymentAsync(
                                            new PaymentRequest
                                            {
                                                Amount = new Amount(Currency.EUR, amount.ToString("F02", CultureInfo.InvariantCulture)),
                                                Description = $"Test Harbour Hop Payment for booking {bookingId}",
                                                //hh web site? thank you page
                                                RedirectUrl = _mollieOptions.RedirectUrl,
                                                WebhookUrl = _mollieOptions.WebhookUrl,
                                                Locale = locale,
                                                //Method = PaymentMethod.CreditCard | PaymentMethod.Ideal | PaymentMethod.PayPal
                                            });

            booking.TransactionId = molliePaymentResponse.Id;

            _bookingPaymentRepository.UpdateBookingPayment(booking);

            return molliePaymentResponse;
        }
        
        public Task<PaymentResponse> GetPaymentAsync(string paymentId)
        {
            return _paymentClient.GetPaymentAsync(paymentId);
        }
    }
}
