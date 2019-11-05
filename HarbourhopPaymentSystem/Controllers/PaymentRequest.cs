using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HarbourhopPaymentSystem.Controllers
{
    public class PaymentRequest
    {
        public int BookingID { get; set; }
        public string CampURL { get; set; }
        public double Amount { get; set; }
    }
}
