using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using PhonebookSoln.Core.Entites;
using PhonebookSoln.Core.Interfaces;
using RabbitMQ.Client;
using System.Text;

namespace PhonebookSoln.Infrastructure.Messaging
{
    public class RabbitMqService : IRabbitMqService
    {
        private readonly string _rabbitMqConnectionString;

        public RabbitMqService(IConfiguration configuration)
        {
            _rabbitMqConnectionString = configuration.GetSection("RabbitMQ:ConnectionString").Value;
        }

        public async Task SendMessageAsync(OutboxMessage message)
        {
            var factory = new ConnectionFactory() { Uri = new Uri(_rabbitMqConnectionString) };

            using (var connection = factory.CreateConnection())
            using (var channel = connection.CreateModel())
            {
                channel.QueueDeclare(queue: "outbox_queue",
                                     durable: true,
                                     exclusive: false,
                                     autoDelete: false,
                                     arguments: null);

                var body = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(message));

                channel.BasicPublish(exchange: "",
                                     routingKey: "outbox_queue",
                                     basicProperties: null,
                                     body: body);
            }
        }
    }
}