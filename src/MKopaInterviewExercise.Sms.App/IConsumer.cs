using System.Threading.Tasks;

namespace MKopaInterviewExercise.Sms.App
{
    public interface IConsumer<IMessage> where IMessage : class
    {
        Task Consume(IMessage message);
    }
}