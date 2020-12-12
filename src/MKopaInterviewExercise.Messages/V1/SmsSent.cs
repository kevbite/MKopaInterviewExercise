using System;

namespace MKopaInterviewExercise.Messages.V1
{
    public record SmsSent(
        Guid MessageId,
        Guid ConversationId,
        string PhoneNumber,
        string MessageText
    );
}