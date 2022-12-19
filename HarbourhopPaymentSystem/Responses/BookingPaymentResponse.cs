using Mollie.Api.Models.Payment;

namespace HarbourhopPaymentSystem.Responses
{
    public class BookingPaymentResponse
    {
        public string PaymentStatus { get; set; }
        public int BookingId { get; set; }
    }
}
