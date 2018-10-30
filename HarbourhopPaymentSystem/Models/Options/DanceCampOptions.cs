namespace HarbourhopPaymentSystem
{
    public class DanceCampOptions
    {
        public string ApiToken { get; set; }

        public string SecretKey { get; set; }

        public string PaymentReceiveDanceCampUrl { get; set; }

        public string PaymentSuccessUrl { get; set; }

        public string PaymentFailedUrl { get; set; }
    }
}
