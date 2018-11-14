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
        public async Task<IActionResult> CreatePayment(int bookingId, double amount)
        {
            try
            {
                //Possible values: en_US nl_NL nl_BE fr_FR fr_BE de_DE de_AT de_CH es_ES ca_ES pt_PT it_IT nb_NO sv_SE fi_FI da_DK is_IS hu_HU pl_PL lv_LV lt_LT
                var molliePaymentResponse = await _paymentService.CreatePayment(bookingId, amount, "en_US");
                return Redirect(molliePaymentResponse.Links.Checkout.Href);
            }
            catch (PaymentAlreadyExistsException)
            {
                return View("Info", $"Payment for booking {bookingId} already exists");
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
                    await _danceCampService.UpdateDanceCampBookingPaymentStatus(paymentId);
                }
                else
                {
                    var status = bookingPayment.PaymentStatus.HasValue ? bookingPayment.PaymentStatus.Value.ToString() : "unknown";
                    _logger.Warning($"Payment for booking id {paymentResponse.BookingId} is unsuccessful with status {status}");
                }
            }
            catch (Exception ex)
            {
                _logger.Error($"An error occured while updating status of payment {paymentId}.", ex);
            }
            return Redirect(_danceCampOptions.PaymentFailedUrl);
        }

        [HttpGet("hello-world")]
        public IActionResult HelloWorld()
        {
            return Ok("HELLO WORLD");
        }
    }
}