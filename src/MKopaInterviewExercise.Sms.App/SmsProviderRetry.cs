using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Polly;

namespace MKopaInterviewExercise.Sms.App
{
    public class SmsProviderRetry : ISmsProvider
    {
        private readonly ISmsProvider _inner;
        private readonly ILogger<SmsProviderRetry> _logger;

        private static readonly HttpStatusCode[] HttpStatusCodesWorthRetrying =
        {
            HttpStatusCode.NotFound, // 404
            HttpStatusCode.BadGateway, // 502
            HttpStatusCode.ServiceUnavailable, // 503
            HttpStatusCode.GatewayTimeout // 504
        };

        public SmsProviderRetry(ISmsProvider inner, ILogger<SmsProviderRetry> logger)
        {
            _inner = inner;
            _logger = logger;
        }

        public async Task<SmsProviderResponse> SendSms(string phoneNumber, string messageText)
        {
            return await Policy
                .HandleResult<SmsProviderResponse>(r => HttpStatusCodesWorthRetrying.Contains(r.HttpStatusCode))
                .RetryAsync(3, onRetry: (exception, retryCount) =>
                {
                    _logger.LogWarning(exception.Exception,
                        "Trying to retry (count: {RetryCount}) sending message to '{PhoneNumber}'", retryCount,
                        phoneNumber);
                })
                .ExecuteAsync(async () => await _inner.SendSms(phoneNumber, messageText));
        }
    }
}