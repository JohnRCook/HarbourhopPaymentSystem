namespace HarbourhopPaymentSystem.Models
{
    public class BookingReportRow
    {
        public int BookingID { get; set; }
        public double TotalCost { get; set; }
        public double Paid { get; set; }
        public double AmountOwed { get; set; }
    }
}
