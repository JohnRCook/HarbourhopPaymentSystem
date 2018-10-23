using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HarbourhopPaymentSystem.Data.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace HarbourhopPaymentSystem.Controllers
{
    [Route("api/[controller]")]
    public class PaymentController : Controller
    {
        private readonly BookingPaymentRepository _bookingPaymentRepository;
        public PaymentController(BookingPaymentRepository bookingPaymentRepository)
        {
            _bookingPaymentRepository = bookingPaymentRepository;
        }

        

        [HttpGet("Create")]
        public IActionResult CreatePayment(int bookingId, double amount)
        {
            _bookingPaymentRepository.AddBookingPayment(new Data.Models.BookingPayment() { BookingId = bookingId, Amount = amount });
            
            return Ok();
        }
    }
}