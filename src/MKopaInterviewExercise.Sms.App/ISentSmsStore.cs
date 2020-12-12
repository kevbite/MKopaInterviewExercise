using System;
using System.Threading.Tasks;

namespace MKopaInterviewExercise.Sms.App
{
    public interface ISentSmsStore
    {
        Task<bool> HasSentSms(Guid messageMessageId);
        Task RecordSentSms(Guid messageId);
    }
}