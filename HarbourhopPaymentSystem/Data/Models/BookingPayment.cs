using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace HarbourhopPaymentSystem.Data.Models
{
    public class BookingPayment
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int BookingId { get; set; }

        [Required]
        public double Amount { get; set; }

        public string TransactionId { get; set; }
    }
}