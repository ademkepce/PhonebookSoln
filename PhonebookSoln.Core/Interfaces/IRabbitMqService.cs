using PhonebookSoln.Core.Entites;

namespace PhonebookSoln.Core.Interfaces
{
    public interface IRabbitMqService
    {
        Task SendMessageAsync(OutboxMessage message);
    }
}
