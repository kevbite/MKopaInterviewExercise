namespace MKopaInterviewExercise.Sms.App
{
    public interface ISmsProviderFactory
    {
        ISmsProvider GetSmsProvider(string phoneNumber);
    }
}