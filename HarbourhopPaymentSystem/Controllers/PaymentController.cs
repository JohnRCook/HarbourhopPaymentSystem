using System;
using System.Threading.Tasks;
using HarbourhopPaymentSystem.Models;
using HarbourhopPaymentSystem.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Mollie.Api.Models.Payment;

namespace HarbourhopPaymentSystem.Controllers
{
    [Route("api/[controller]")]
    public class PaymentController : Controller
    {
        private readonly PaymentService _paymentService;
        private readonly DanceCampService _danceCampService;
        private readonly DanceCampOptions _danceCampOptions;

        public PaymentController(PaymentService paymentService, DanceCampService danceCampService, IOptionsSnapshot<DanceCampOptions> danceCampOptions)
        {
            _paymentService = paymentService;
            _danceCampService = danceCampService;
            _danceCampOptions = danceCampOptions.Value;
        }

        [HttpGet("create")]
        public async Task<IActionResult> CreatePayment(int bookingId, double amount)
        {
            try
            {
                //Possible values: en_US nl_NL nl_BE fr_FR fr_BE de_DE de_AT de_CH es_ES ca_ES pt_PT it_IT nb_NO sv_SE fi_FI da_DK is_IS hu_HU pl_PL lv_LV lt_LT
                var molliePaymentResponse = await _paymentService.CreatePayment(bookingId, amount, "nl_NL");
                return Redirect(molliePaymentResponse.Links.Checkout.Href);
            }
            catch (BookingAlreadyExistsException)
            {
                return Conflict();
            }
        }

        [HttpPost("status")]
        public async Task<IActionResult> UpdateStatus()
        {
            string paymentId = Request.Form["id"];

            try
            {
                var paymentResponse = await _paymentService.GetPaymentAsync(paymentId);

                var status = paymentResponse.Status;

                if (status.HasValue && status == PaymentStatus.Paid)
                {
                    //await _danceCampService.UpdateDanceCampBookingPaymentStatus(paymentId);
                    return Redirect(_danceCampOptions.PaymentSuccessUrl);
                }
            }
            catch(Exception ex)
            {

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