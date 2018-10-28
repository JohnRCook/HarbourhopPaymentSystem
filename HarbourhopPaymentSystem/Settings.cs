namespace HarbourhopPaymentSystem
{
    public class Settings
    {
        public string DefaultConnection { get; set; }
        public string MollieApiKey { get; set; }
        public bool TestMode { get; set; }
        public string TotpApiSecretKey { get; set; }
        public string PaymentReceiveDanceCampUrl { get; set; }
        public string PaymentSuccessUrl { get; set; }
        public string PaymentFailedUrl { get; set; }
    }
}
