using Microsoft.Extensions.Configuration;
using PhonebookSoln.Core.Entites;
using RabbitMQ.Client.Events;
using RabbitMQ.Client;
using System.Text;
using Newtonsoft.Json;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using PhonebookSoln.Application.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace PhonebookSoln.Infrastructure.Consumer
{
    public class RabbitMqConsumer : BackgroundService
    {
        private readonly string _rabbitMqConnectionString;
        private readonly ILogger<RabbitMqConsumer> _logger;
        private readonly IServiceProvider _serviceProvider;

        public RabbitMqConsumer(IConfiguration configuration, ILogger<RabbitMqConsumer> logger, IServiceProvider serviceProvider)
        {
            _rabbitMqConnectionString = configuration.GetSection("RabbitMQ:ConnectionString").Value;
            _logger = logger;
            _serviceProvider = serviceProvider;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var factory = new ConnectionFactory() { Uri = new Uri(_rabbitMqConnectionString) };

            using (var connection = factory.CreateConnection())
            using (var channel = connection.CreateModel())
            {
                channel.QueueDeclare(queue: "outbox_queue", durable: true, exclusive: false, autoDelete: false, arguments: null);

                var consumer = new EventingBasicConsumer(channel);
                consumer.Received += async (model, ea) =>
                {
                    var body = ea.Body.ToArray();
                    var messageJson = Encoding.UTF8.GetString(body);
                    var message = JsonConvert.DeserializeObject<OutboxMessage>(messageJson);

                    try
                    {
                        await ProcessMessageAsync(message);

                        channel.BasicAck(deliveryTag: ea.DeliveryTag, multiple: false);
                    }
                    catch (JsonSerializationException jsonEx)
                    {
                        _logger.LogError($"Error deserializing message {message.Id}: {jsonEx.Message}");
                        channel.BasicNack(deliveryTag: ea.DeliveryTag, multiple: false, requeue: false);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError($"Error processing message {message.Id}: {ex.Message}");
                        channel.BasicNack(deliveryTag: ea.DeliveryTag, multiple: false, requeue: true);
                    }
                };

                channel.BasicConsume(queue: "outbox_queue", autoAck: false, consumer: consumer);

                await Task.Delay(-1, stoppingToken);
            }
        }

        private async Task ProcessMessageAsync(OutboxMessage message)
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                var reportService = scope.ServiceProvider.GetRequiredService<IReportService>();

                try
                {
                    if (!Guid.TryParse(message.Payload, out Guid reportId))
                    {
                        _logger.LogError($"Invalid GUID format in message {message.Id}: {message.Payload}");
                        return;
                    }

                    await reportService.GenerateReportAsync(reportId);

                    var outboxService = scope.ServiceProvider.GetRequiredService<IOutboxService>();
                    await outboxService.MarkMessageAsProcessedAsync(message.Id);

                    _logger.LogInformation($"Successfully processed and generated report with ID {reportId}.");
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Error processing message {message.Id}: {ex.Message}");
                    throw;
                }
            }
        }
    }
}
