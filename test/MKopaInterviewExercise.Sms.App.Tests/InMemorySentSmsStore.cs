using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MKopaInterviewExercise.Sms.App.Tests
{
    public class InMemorySentSmsStore : ISentSmsStore
    {
        private HashSet<Guid> _messageIds = new();

        public Task<bool> HasSentSms(Guid messageMessageId)
        {
            return Task.FromResult(_messageIds.Contains(messageMessageId));
        }

        public Task RecordSentSms(Guid messageId)
        {
            _messageIds.Add(messageId);
            return Task.CompletedTask;
        }
    }
}