using System;

namespace MKopaInterviewExercise.Sms.Messages.V1
{
    public record SendSms(
        Guid MessageId,
        Guid ConversationId,
        string PhoneNumber,
        string MessageText
    );
}