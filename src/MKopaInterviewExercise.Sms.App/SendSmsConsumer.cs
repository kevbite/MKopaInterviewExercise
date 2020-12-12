using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using MKopaInterviewExercise.Messages.V1;
using MKopaInterviewExercise.Sms.Messages.V1;

namespace MKopaInterviewExercise.Sms.App
{
    public class SendSmsConsumer : IConsumer<SendSms>
    {
        private readonly IGlobalEventPublisher _globalEventPublisher;
        private readonly ISmsProviderFactory _smsProviderFactory;
        private readonly ISentSmsStore _store;
        private readonly ILogger<SendSmsConsumer> _logger;

        public SendSmsConsumer(IGlobalEventPublisher globalEventPublisher,
            ISmsProviderFactory smsProviderFactory,
            ISentSmsStore store,
            ILogger<SendSmsConsumer> logger)
        {
            _globalEventPublisher = globalEventPublisher;
            _smsProviderFactory = smsProviderFactory;
            _store = store;
            _logger = logger;
        }

        public async Task Consume(SendSms message)
        {
            if (await _store.HasSentSms(message.MessageId))
            {
                _logger.LogWarning($"SendSms command '{message.MessageId}' has already been consumed before.");
                return;
            }

            var smsProvider =
                _smsProviderFactory.GetSmsProvider(message.PhoneNumber);

            await smsProvider.SendSms(message.PhoneNumber, message.MessageText);

            await _store.RecordSentSms(message.MessageId);

            await _globalEventPublisher.Publish(new SmsSent(
                Guid.NewGuid(),
                message.ConversationId,
                message.PhoneNumber,
                message.MessageText));
        }
    }
}