using Mollie.Api.Models.Payment;

namespace HarbourhopPaymentSystem.Responses
{
    public class BookingPaymentResponse
    {
        public PaymentStatus? PaymentStatus { get; set; }
        public int BookingId { get; set; }
    }
}
