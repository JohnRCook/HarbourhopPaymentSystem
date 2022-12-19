using System;
using System.Threading.Tasks;
using HarbourhopPaymentSystem.Models;
using HarbourhopPaymentSystem.Services;
using Microsoft.AspNetCore.Mvc;
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

        public PaymentController(PaymentService paymentService, DanceCampService danceCampService, ILogger logger)
        {
            _paymentService = paymentService;
            _danceCampService = danceCampService;
            _logger = logger;
        }

        [HttpGet("create")]
        public async Task<IActionResult> CreatePayment(int bookingId)
        {
            return await ExecuteCreatePayment(bookingId);
        }

        [HttpPost("create")]
        public async Task<IActionResult> CreatePaymentAfterRegistration(PaymentRequest request)
        {
            return await ExecuteCreatePayment(request.BookingID);
        }

        private async Task<IActionResult> ExecuteCreatePayment(int bookingId)
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
                return View("PaymentExists", $"Payment for booking {bookingId} already exists");
            }
            catch (BookingNotFoundException)
            {
                return View("BookingNotFound", $"Booking with {bookingId} was not found in the system");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "An error occured while creating a payment for booking id {BookingId}", bookingId);

                return View("PaymentError", $"An error occured while creating a payment for booking id {bookingId}");
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
                
                if (bookingPayment.PaymentStatus == PaymentStatus.Paid)
                {
                    var status = bookingPayment.PaymentStatus;
                    _paymentService.SetBookingPaymentStatus(bookingPayment.BookingId, success: true, status);
                    await _danceCampService.UpdateDanceCampBookingPaymentStatus(bookingPayment.BookingId, paymentId, success: true, status);
                }
                else
                {
                    _logger.Warning("Payment for booking id {PaymentResponseBookingId} is unsuccessful with status {BookingPaymentPaymentStatus}",
                        paymentResponse.BookingId, bookingPayment.PaymentStatus);

                    _paymentService.SetBookingPaymentStatus(bookingPayment.BookingId, success:false, bookingPayment.PaymentStatus);

                    // Only send a message to dancecamps if the payment was succesful.
                    //await _danceCampService.UpdateDanceCampBookingPaymentStatus(paymentId, success:false, status);
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex,"An error occured while updating status of payment {PaymentId}. Error message: {ExMessage}", paymentId, ex.Message);
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