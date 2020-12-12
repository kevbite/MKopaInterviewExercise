using System.Threading.Tasks;

namespace MKopaInterviewExercise.Sms.App
{
    public interface IGlobalEventPublisher
    {
        Task Publish<TMessage>(TMessage message) where TMessage : class;
    }
}