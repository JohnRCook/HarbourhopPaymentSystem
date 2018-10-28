using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using HarbourhopPaymentSystem.Data.Repositories;
using HarbourhopPaymentSystem.Responses;
using Microsoft.AspNetCore.Mvc;
using Mollie.Api.Client.Abstract;
using Mollie.Api.Models;
using Mollie.Api.Models.Payment;
using Mollie.Api.Models.Payment.Request;
using Newtonsoft.Json;
using OtpNet;

namespace HarbourhopPaymentSystem.Controllers
{
    [Route("api/[controller]")]
    public class PaymentController: Controller
    {
        private readonly BookingPaymentRepository _bookingPaymentRepository;
        private readonly IPaymentClient _paymentClient;
        private readonly Settings _settings;

        public PaymentController(BookingPaymentRepository bookingPaymentRepository, IPaymentClient paymentClient, Settings settings)
        {
            _bookingPaymentRepository = bookingPaymentRepository;
            _paymentClient = paymentClient;
            _settings = settings;
        }

        [HttpGet("create")]
        public async Task<IActionResult> CreatePayment(int bookingId, double amount)
        {
            //TODO: if booking exits? what is the scenario? 

            var booking = _bookingPaymentRepository.GetBookingPayment(bookingId);

            if(!string.IsNullOrEmpty(booking?.TransactionId))
                return Conflict();

            if(booking == null)
            {
                booking = _bookingPaymentRepository.AddBookingPayment(new Data.Models.BookingPayment { BookingId = bookingId, Amount = amount });
            }

            var molliePaymentResponse = await _paymentClient.CreatePaymentAsync(
                                            new PaymentRequest
                                            {
                                                Amount = new Amount(Currency.EUR, amount.ToString("F02")),
                                                Description = $"Test Harbour Hop Payment for booking {bookingId}",
                                                //hh web site? thank you page
                                                RedirectUrl = "https://www.harbourhop.nl/"
                                            });

            booking.TransactionId = molliePaymentResponse.Id;

            _bookingPaymentRepository.UpdateBookingPayment(booking);

            return Redirect(molliePaymentResponse.Links.Checkout.Href);
        }

        [HttpPost("status")]
        public async Task<IActionResult> UpdateStatus()
        {
            string id = Request.Form["id"];

            var paymentResponse = await _paymentClient.GetPaymentAsync(id, _settings.TestMode);

            var status = paymentResponse.Status;

            if(status.HasValue && status == PaymentStatus.Paid)
            {
                await UpdateDanceCampBookingPaymentStatus();
                return Redirect(_settings.PaymentSuccessUrl);
            }

            return Redirect(_settings.PaymentFailedUrl);

            async Task UpdateDanceCampBookingPaymentStatus()
            {
                try
                {
                    var booking = _bookingPaymentRepository.GetBookingPayment(id);

                    byte[] secretKeyByteArray = Base32Encoding.ToBytes(_settings.TotpApiSecretKey);

                    var totp = new Totp(secretKeyByteArray, 30, OtpHashMode.Sha1, 6);
                    var totpCode = totp.ComputeTotp();

                    var formContent = new FormUrlEncodedContent(
                        new[]
                        {
                            new KeyValuePair<string, string>("BookingID", booking.BookingId.ToString()),
                            new KeyValuePair<string, string>("APIToken", totpCode),
                            new KeyValuePair<string, string>("Token", totpCode),
                            new KeyValuePair<string, string>("Status", "Completed"),
                            new KeyValuePair<string, string>("Amount", booking.Amount.ToString("F02")),
                            new KeyValuePair<string, string>("Currency", "EUR"),
                            new KeyValuePair<string, string>("TxnID", id),
                            new KeyValuePair<string, string>("FeeAmount", "0"),
                            new KeyValuePair<string, string>("Notes", ""),
                            new KeyValuePair<string, string>("EmailNotes", "")
                        });

                    var myHttpClient = new HttpClient();

                    var response = await myHttpClient.PostAsync(_settings.PaymentReceiveDanceCampUrl, formContent);

                    if(response.StatusCode == HttpStatusCode.OK)
                    {
                        var stringContent = await response.Content.ReadAsStringAsync();

                        var result = JsonConvert.DeserializeObject<DanceCampResponse>(stringContent);

                        if(result.Status != "success")
                        {
                            //TODO: discuss notification options. 
                            //There is a chance that mollie payment is successful but update to dancecamp system failed
                            //Generate notification email? 
                        }
                    }
                }
                catch(Exception e)
                {
                    //TODO: discuss notification options. 
                    //There is a chance that mollie payment is successful but update to dancecamp system failed
                    //Generate notification email? 
                }
            }
        }
    }
}