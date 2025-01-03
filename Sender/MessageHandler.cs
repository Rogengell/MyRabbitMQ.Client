using EasyNetQ;
using SharedMessages;

namespace Sender;

public class MessageHandler : BackgroundService
{
    private void HandlePongMessage(PongMessage message)
    {
        Console.WriteLine($"Got pong: {message.Message}");
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var client = new MessageClient(RabbitHutch.CreateBus("host=rabbitmq;port=5672;virtualHost=/;username=guest;password=guest"));

        await client.Listen<PongMessage>("Order", HandlePongMessage, "Inventory");

        await Task.Delay(-1, stoppingToken);
    }
}
