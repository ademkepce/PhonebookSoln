using PhonebookSoln.Core.Entites;

namespace PhonebookSoln.Application.Interfaces
{
    public interface IOutboxService
    {
        // İşlenmemiş Outbox mesajlarını işlemeye başlar
        Task ProcessUnprocessedMessagesAsync();

        // Tek bir Outbox mesajını işler
        Task ProcessMessageAsync(OutboxMessage message);

        // Mesajı RabbitMQ'ya gönderir
        Task SendToRabbitMqAsync(OutboxMessage message);

        // Mesajı "işlenmiş" olarak işaretler
        Task MarkMessageAsProcessedAsync(Guid messageId);
    }
}
