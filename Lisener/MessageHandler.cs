using EasyNetQ;
using SharedMessages;

namespace Lisener;

public class MessageHandler : BackgroundService
{
    private async void HandlePongMessage(PongMessage message)
    {
        await Task.Delay(5000);
        Console.WriteLine($"Got pong: {message.Message}");

        var client = new MessageClient(RabbitHutch.CreateBus("host=rabbitmq;port=5672;virtualHost=/;username=guest;password=guest"));
        await client.Send<bool>(true, message.CorrelationId);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var client = new MessageClient(RabbitHutch.CreateBus("host=rabbitmq;port=5672;virtualHost=/;username=guest;password=guest"));

        await client.Listen<PongMessage>("Inventory", HandlePongMessage, "Order");

        await Task.Delay(-1, stoppingToken);
    }
}