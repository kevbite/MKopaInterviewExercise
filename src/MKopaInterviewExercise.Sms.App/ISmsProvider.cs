using System.Threading.Tasks;

namespace MKopaInterviewExercise.Sms.App
{
    public interface ISmsProvider
    {
        Task<SmsProviderResponse> SendSms(string phoneNumber, string messageText);
    }
}