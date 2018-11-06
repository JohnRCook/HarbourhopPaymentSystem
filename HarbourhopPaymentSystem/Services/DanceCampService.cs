using HarbourhopPaymentSystem.Data.Repositories;
using HarbourhopPaymentSystem.Responses;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using OtpNet;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace HarbourhopPaymentSystem.Services
{
    public class DanceCampService
    {
        private readonly BookingPaymentRepository _bookingPaymentRepository;
        private readonly ILogger<DanceCampService> _logger;
        private readonly DanceCampOptions _danceCampOptions;


        public DanceCampService(BookingPaymentRepository bookingPaymentRepository, IOptionsSnapshot<DanceCampOptions> danceCampOptions, ILogger<DanceCampService> logger)
        {
            _bookingPaymentRepository = bookingPaymentRepository;
            _logger = logger;
            _danceCampOptions = danceCampOptions.Value;
        }

        public async Task UpdateDanceCampBookingPaymentStatus(string transactionId)
        {
            try
            {
                _logger.LogInformation($"transactionID: {transactionId}");
                var booking = _bookingPaymentRepository.GetBookingPayment(transactionId);

                _logger.LogInformation($"booking ID is: {booking.BookingId}");

                byte[] secretKeyByteArray = Base32Encoding.ToBytes(_danceCampOptions.SecretKey);

                var totp = new Totp(secretKeyByteArray, 30, OtpHashMode.Sha1, 6);
                var totpCode = totp.ComputeTotp();

                var formContent = new FormUrlEncodedContent(
                    new[]
                    {
                            new KeyValuePair<string, string>("BookingID", booking.BookingId.ToString()),
                            new KeyValuePair<string, string>("APIToken", _danceCampOptions.ApiToken),
                            new KeyValuePair<string, string>("Token", totpCode),
                            new KeyValuePair<string, string>("Status", "Completed"),
                            new KeyValuePair<string, string>("Amount", booking.Amount.ToString("F02", CultureInfo.InvariantCulture)),
                            new KeyValuePair<string, string>("Currency", "EUR"),
                            new KeyValuePair<string, string>("TxnID", transactionId),
                            new KeyValuePair<string, string>("FeeAmount", "0"),
                            new KeyValuePair<string, string>("Notes", ""),
                            new KeyValuePair<string, string>("EmailNotes", "")
                    });

                var myHttpClient = new HttpClient();

                var response = await myHttpClient.PostAsync(_danceCampOptions.PaymentReceiveDanceCampUrl, formContent);

                if (response.StatusCode == HttpStatusCode.OK)
                {
                    var stringContent = await response.Content.ReadAsStringAsync();

                    var result = JsonConvert.DeserializeObject<DanceCampResponse>(stringContent);

                    if (result.Status != "success")
                    {
                        //TODO: discuss notification options. 
                        //There is a chance that mollie payment is successful but update to dancecamp system failed
                        //Generate notification email? 
                        _logger.LogError("Response from Dance Camp was not succesfull", result);
                    }
                }
                else
                {
                    _logger.LogError("Request to Dance Camp was not successfull", response);
                }
            }
            catch (Exception e)
            {
                //TODO: discuss notification options. 
                //There is a chance that mollie payment is successful but update to dancecamp system failed
                //Generate notification email?
                _logger.LogError("Error occured while sending Payment Receive to Dance Camp", e);
            }
        }
    }
}
