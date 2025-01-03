using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using SharedMessages;
namespace Sender
{
    public class RabbitMqClient
    {
        private readonly IConnection _connection;
        private readonly IModel _channel;

        public RabbitMqClient()
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

        public async Task<bool> SendAndReceiveAsync(object message, string routingKey, int timeoutSeconds = 30)
        {
            var replyQueueName = _channel.QueueDeclare().QueueName;
            var correlationId = Guid.NewGuid().ToString();
            var tcs = new TaskCompletionSource<bool>();

            var consumer = new EventingBasicConsumer(_channel);
            consumer.Received += (model, ea) =>
            {
                if (ea.BasicProperties.CorrelationId == correlationId)
                {
                    var responseMessage = Encoding.UTF8.GetString(ea.Body.ToArray());
                    var response = bool.Parse(responseMessage);
                    tcs.TrySetResult(response);
                }
            };

            var consumerTag = _channel.BasicConsume(consumer: consumer, queue: replyQueueName, autoAck: true);

            var properties = _channel.CreateBasicProperties();
            properties.CorrelationId = correlationId;
            properties.ReplyTo = replyQueueName;

            var messageBody = Encoding.UTF8.GetBytes(message.ToString());
            _channel.BasicPublish(exchange: "my_exchange", routingKey: routingKey, basicProperties: properties, body: messageBody);

            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(timeoutSeconds));
            cts.Token.Register(() => tcs.TrySetCanceled(), useSynchronizationContext: false);

            try
            {
                return await tcs.Task.ConfigureAwait(false);
            }
            catch (TaskCanceledException)
            {
                Console.WriteLine("Timeout occurred. Cancelling consumer.");
                return false;
            }
            finally
            {
                _channel.BasicCancel(consumerTag);
            }
        }

        public void Dispose()
        {
            _channel?.Dispose();
            _connection?.Dispose();
        }
    }
}