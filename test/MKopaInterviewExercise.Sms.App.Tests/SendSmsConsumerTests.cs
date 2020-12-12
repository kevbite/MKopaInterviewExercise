using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using FluentAssertions;
using FluentAssertions.Execution;
using Microsoft.Extensions.Logging.Abstractions;
using MKopaInterviewExercise.Messages.V1;
using MKopaInterviewExercise.Sms.Messages.V1;
using Xunit;

namespace MKopaInterviewExercise.Sms.App.Tests
{
    public class SendSmsConsumerTests : IGlobalEventPublisher, ISmsProviderFactory, ISmsProvider
    {
        private readonly SendSmsConsumer _consumer;
        private readonly List<object> _publishedMessages = new();
        private readonly List<SentSmsMessage> _sentSmsMessages = new();
        private readonly InMemorySentSmsStore _store;
        private readonly Queue<SmsProviderResponse> _smsProviderResponses = new();

        public SendSmsConsumerTests()
        {
            _store = new InMemorySentSmsStore();
            _consumer = new(this, new SmsProviderFactory(() => this, new NullLoggerFactory()), _store, new NullLogger<SendSmsConsumer>());
        }

        [Fact]
        public async Task ShouldPublishSmsSentMessage()
        {
            var command = CreateSendSmsCommand();
            await _consumer.Consume(command);

            using var _ = new AssertionScope();
            _publishedMessages.Should().AllBeOfType<SmsSent>();
            _publishedMessages.Should().BeEquivalentTo(
                new
                {
                    command.ConversationId,
                    command.MessageText,
                    command.PhoneNumber
                }
            );
        }

        [Fact]
        public async Task ShouldPublishSmsSentMessagesWithUniqueMessageIds()
        {
            await _consumer.Consume(CreateSendSmsCommand());
            await _consumer.Consume(CreateSendSmsCommand());
            await _consumer.Consume(CreateSendSmsCommand());

            _publishedMessages.Cast<SmsSent>()
                .Select(x => x.MessageId)
                .Should()
                .OnlyHaveUniqueItems();
        }

        [Fact]
        public async Task ShouldCallSmsProvider()
        {
            var sendSmsCommand = CreateSendSmsCommand();
            await _consumer.Consume(sendSmsCommand);

            _sentSmsMessages.Should().BeEquivalentTo(new
            {
                sendSmsCommand.PhoneNumber,
                sendSmsCommand.MessageText
            });
        }

        [Fact]
        public async Task ShouldStoreSeenMessageId()
        {
            var sendSmsCommand = CreateSendSmsCommand();
            await _consumer.Consume(sendSmsCommand);

            (await _store.HasSentSms(sendSmsCommand.MessageId))
                .Should().BeTrue();
        }

        [Fact]
        public async Task ShouldNotPublishSmsSentOrSendSmsToProvider()
        {
            var messageId = Guid.NewGuid();
            await _store.RecordSentSms(messageId);

            var sendSmsCommand = CreateSendSmsCommand()
                with{
                    MessageId = messageId
                };
            await _consumer.Consume(sendSmsCommand);

            using var _ = new AssertionScope();
            _publishedMessages.Should().BeEmpty();
            _sentSmsMessages.Should().BeEmpty();
        }
        
        [Theory]
        [InlineData(HttpStatusCode.NotFound)]
        [InlineData(HttpStatusCode.BadGateway)]
        [InlineData(HttpStatusCode.ServiceUnavailable)]
        [InlineData(HttpStatusCode.GatewayTimeout)]
        public async Task ShouldDealWithRetriesToSmsProvider(HttpStatusCode httpStatusCode)
        {
            _smsProviderResponses.Enqueue(new SmsProviderResponse(httpStatusCode));
             _smsProviderResponses.Enqueue(new SmsProviderResponse(HttpStatusCode.OK));

            var sendSmsCommand = CreateSendSmsCommand();
            await _consumer.Consume(sendSmsCommand);

            using var _ = new AssertionScope();
            _publishedMessages.Should().HaveCount(1);
            _sentSmsMessages.Should().HaveCount(2);
        }

        private static SendSms CreateSendSmsCommand()
        {
            return new(
                Guid.Empty,
                Guid.NewGuid(),
                "+447728199472",
                $"{Guid.NewGuid()}");
        }

        Task IGlobalEventPublisher.Publish<TMessage>(TMessage message) where TMessage : class
        {
            _publishedMessages.Add(message);
            return Task.CompletedTask;
        }

        ISmsProvider ISmsProviderFactory.GetSmsProvider(string phoneNumber)
        {
            return this;
        }

        Task<SmsProviderResponse> ISmsProvider.SendSms(string phoneNumber, string messageText)
        {
            _sentSmsMessages.Add(new SentSmsMessage(phoneNumber, messageText));
            if (!_smsProviderResponses.TryDequeue(out var result))
            {
                result = new SmsProviderResponse(HttpStatusCode.OK);
            }
            return Task.FromResult(result);
        }

        record SentSmsMessage(string PhoneNumber, string MessageText);
    }
}