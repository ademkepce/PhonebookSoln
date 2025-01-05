using Microsoft.Extensions.Logging;
using PhonebookSoln.Application.Interfaces;
using PhonebookSoln.Core.Entites;
using PhonebookSoln.Core.Interfaces;

namespace PhonebookSoln.Application.Services
{
    public class OutboxService : IOutboxService
    {
        private readonly IPhonebookRepository _phonebookRepository;
        private readonly IRabbitMqService _rabbitMqService;
        private readonly ILogger<OutboxService> _logger;

        public OutboxService(IPhonebookRepository phonebookRepository, IRabbitMqService rabbitMqService, ILogger<OutboxService> logger)
        {
            _phonebookRepository = phonebookRepository;
            _rabbitMqService = rabbitMqService;
            _logger = logger;
        }

        public async Task ProcessUnprocessedMessagesAsync()
        {
            var unprocessedMessages = await _phonebookRepository.GetUnprocessedOutboxMessagesAsync();

            if (!unprocessedMessages.Any())
            {
                _logger.LogInformation("No unprocessed outbox messages found.");
                return;
            }

            foreach (var message in unprocessedMessages)
            {
                try
                {
                    await ProcessMessageAsync(message);
                    await MarkMessageAsProcessedAsync(message.Id);
                    _logger.LogInformation($"Message with ID {message.Id} processed successfully.");
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Error processing message with ID {message.Id}: {ex.Message}");
                }
            }
        }

        public async Task ProcessMessageAsync(OutboxMessage message)
        {
            await SendToRabbitMqAsync(message);
        }

        public async Task SendToRabbitMqAsync(OutboxMessage message)
        {
            try
            {
                await _rabbitMqService.SendMessageAsync(message);
                _logger.LogInformation($"Message with ID {message.Id} sent to RabbitMQ.");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed to send message with ID {message.Id} to RabbitMQ: {ex.Message}");
                throw;
            }
        }

        public async Task MarkMessageAsProcessedAsync(Guid messageId)
        {
            try
            {
                await _phonebookRepository.MarkOutboxMessageAsProcessedAsync(messageId);
                _logger.LogInformation($"Message with ID {messageId} marked as processed.");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed to mark message with ID {messageId} as processed: {ex.Message}");
                throw;
            }
        }
    }
}
