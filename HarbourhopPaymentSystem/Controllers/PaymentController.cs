using System;
using System.Threading.Tasks;
using HarbourhopPaymentSystem.Models;
using HarbourhopPaymentSystem.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Mollie.Api.Models.Payment;
using Serilog;

namespace HarbourhopPaymentSystem.Controllers
{
    [Route("api/[controller]")]
    public class PaymentController : Controller
    {
        private readonly PaymentService _paymentService;
        private readonly DanceCampService _danceCampService;
        private readonly ILogger _logger;
        private readonly DanceCampOptions _danceCampOptions;

        public PaymentController(PaymentService paymentService, DanceCampService danceCampService, IOptionsSnapshot<DanceCampOptions> danceCampOptions, ILogger logger)
        {
            _paymentService = paymentService;
            _danceCampService = danceCampService;
            _logger = logger;
            _danceCampOptions = danceCampOptions.Value;
        }

        [HttpGet("create")]
        public async Task<IActionResult> CreatePayment(int bookingId)
        {
            try
            {
                var amountOwed = await _danceCampService.GetOwedBookingAmount(bookingId);

                await _paymentService.ValidateBookingPayment(bookingId, amountOwed);

                var molliePaymentResponse = await _paymentService.CreatePayment(bookingId, amountOwed, "en_US");
                return Redirect(molliePaymentResponse.Links.Checkout.Href);
            }
            catch (PaymentAlreadyExistsException)
            {
                return View("Info", $"Payment for booking {bookingId} already exists");
            }
            catch (BookingNotFoundException)
            {
                return View("Info", $"Booking with {bookingId} was not found in the system");
            }
            catch (Exception ex)
            {
                var errorMessage = $"An error occured while creating a payment for booking id {bookingId}";
                _logger.Error(errorMessage, ex);

                return View("Info", errorMessage);
            }
        }

        [HttpPost("status")]
        public async Task<IActionResult> UpdateStatus()
        {
            string paymentId = Request.Form["id"];

            try
            {
                var paymentResponse = await _paymentService.GetPaymentAsync(paymentId);

                var bookingPayment = paymentResponse;

                if (bookingPayment.PaymentStatus.HasValue && bookingPayment.PaymentStatus == PaymentStatus.Paid)
                {
                    _paymentService.SetBookingPaymentStatus(bookingPayment.BookingId, success: true);
                    await _danceCampService.UpdateDanceCampBookingPaymentStatus(paymentId, success: true);
                }
                else
                {
                    var status = bookingPayment.PaymentStatus.HasValue ? bookingPayment.PaymentStatus.Value.ToString() : "unknown";
                    _logger.Warning($"Payment for booking id {paymentResponse.BookingId} is unsuccessful with status {status}");

                    _paymentService.SetBookingPaymentStatus(bookingPayment.BookingId, success: false);
                    await _danceCampService.UpdateDanceCampBookingPaymentStatus(paymentId, success: false);
                }
            }
            catch (Exception ex)
            {
                _logger.Error($"An error occured while updating status of payment {paymentId}.", ex);
            }
            return Ok();
        }

        [HttpGet("hello-world")]
        public IActionResult HelloWorld()
        {
            return Ok("HELLO WORLD");
        }
    }
}