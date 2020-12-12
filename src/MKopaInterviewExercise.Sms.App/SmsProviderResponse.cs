using System.Net;

namespace MKopaInterviewExercise.Sms.App
{
    public record SmsProviderResponse(
        HttpStatusCode HttpStatusCode
    );
}