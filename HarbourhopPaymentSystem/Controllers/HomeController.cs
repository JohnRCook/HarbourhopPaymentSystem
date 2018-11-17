using HarbourhopPaymentSystem.Models;
using HarbourhopPaymentSystem.Services;
using Microsoft.AspNetCore.Mvc;

namespace HarbourhopPaymentSystem.Controllers
{
    public class HomeController : Controller
    {
        private readonly PaymentService _paymentService;

        public HomeController(PaymentService paymentService)
        {
            _paymentService = paymentService;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpGet("/PaymentStatus/{bookingId}")]
        public IActionResult PaymentStatus(int bookingId)
        {
            var bookingPayment = _paymentService.GetBookingPayment(bookingId);
            
            var paymentStatus = new PaymentStatusViewModel { Success = bookingPayment != null && bookingPayment.Success.HasValue && bookingPayment.Success.Value };
            return View(paymentStatus);
        }
    }
}