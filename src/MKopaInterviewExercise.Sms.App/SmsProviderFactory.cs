using System;
using Microsoft.Extensions.Logging;

namespace MKopaInterviewExercise.Sms.App
{
    public class SmsProviderFactory : ISmsProviderFactory
    {
        private readonly Func<ISmsProvider> _defaultProvider;
        private readonly ILoggerFactory _loggerFactory;

        public SmsProviderFactory(Func<ISmsProvider> defaultProvider, ILoggerFactory loggerFactory)
        {
            _defaultProvider = defaultProvider;
            _loggerFactory = loggerFactory;
        }
        public ISmsProvider GetSmsProvider(string phoneNumber)
        {
            var defaultProvider = _defaultProvider();
            var logger = _loggerFactory.CreateLogger<SmsProviderRetry>();
            
            return new SmsProviderRetry(defaultProvider, logger);
        }
    }
}