using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using SharedMessages;

namespace Lisener;

public class MessageHandler : BackgroundService
{
    private readonly IConnection _connection;
    private readonly IModel _channel;

    public MessageHandler()
    {
        var factory = new ConnectionFactory
            {
                HostName = "rabbitmq",
                Port = 5672,
                UserName = "guest",
                Password = "guest"
            };
        _connection = factory.CreateConnection();
        _channel = _connection.CreateModel();
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var routingKeys = new[] { "key1", "key2", "key3" };

        // Declare the exchange to ensure it exists
        _channel.ExchangeDeclare(exchange: "my_exchange", type: ExchangeType.Direct, durable: false, autoDelete: false);

        foreach (var routingKey in routingKeys)
        {
            var queueName = $"queue_{routingKey}";
            _channel.QueueDeclare(queue: queueName, durable: false, exclusive: false, autoDelete: false);

            // Bind the queue to the exchange with the routing key
            _channel.QueueBind(queue: queueName, exchange: "my_exchange", routingKey: routingKey);

            var consumer = new EventingBasicConsumer(_channel);
            consumer.Received += (model, ea) =>
            {
                if (stoppingToken.IsCancellationRequested)
                {
                    return;
                }

                var message = Encoding.UTF8.GetString(ea.Body.ToArray());
                Console.WriteLine($"Received message: {message} on routing key: {routingKey}");

                var response = ProcessMessage(message, routingKey);

                var replyProps = _channel.CreateBasicProperties();
                replyProps.CorrelationId = ea.BasicProperties.CorrelationId;

                var responseBytes = Encoding.UTF8.GetBytes(response.ToString());
                _channel.BasicPublish(exchange: "",
                                    routingKey: ea.BasicProperties.ReplyTo,
                                    basicProperties: replyProps,
                                    body: responseBytes);
            };

            _channel.BasicConsume(queue: queueName, autoAck: true, consumer: consumer);
        }

        return Task.CompletedTask;
    }

    private bool ProcessMessage(string message, string routingKey)
    {
        // Business logic for processing the message
        return true; // Example response
    }

    public override void Dispose()
    {
        _channel?.Dispose();
        _connection?.Dispose();
        base.Dispose();
    }
}