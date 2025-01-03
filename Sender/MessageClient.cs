using EasyNetQ;
using EasyNetQ.Consumer;
using RabbitMQ.Client;
using SharedMessages;

namespace Sender;

public class MessageClient
{
    private readonly IBus _bus;
    
    public MessageClient(IBus bus)
    {
        _bus = bus;
    }

    public MessageClient()
    {
        
    }

    public virtual async Task Send<T>(T message, string routingKey)
    {
        await _bus.PubSub.PublishAsync(message, routingKey);
    }

    public async Task<bool> Responce(string CorrelationIdGenerated)
    {
        var tcs = new TaskCompletionSource<bool>();

        await _bus.PubSub.SubscribeAsync<bool>("ResponceQueue", async message =>
        {
            // Set the TaskCompletionSource result based on the message
            tcs.SetResult(message);
        },config => config.WithTopic(CorrelationIdGenerated));

        // Wait for the message to come in (with a timeout)
        var timeout = Task.WhenAny(tcs.Task, Task.Delay(30000)); // 30 seconds timeout

        var completedTask = await timeout;
        
        if (completedTask == tcs.Task)
        {
            // Message received, process it
            bool result = await tcs.Task;

            if (result)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        else
        {
            // Timeout occurred
            return false;
        }
    }

    public async Task Listen<T>(String Queue, Action<T> handler, string routingKey)
    {
        await _bus.PubSub.SubscribeAsync<T>(Queue, handler, config => config.WithTopic(routingKey));
    }
}