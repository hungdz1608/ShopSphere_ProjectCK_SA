using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using ShopSphere.Application.Messaging;

namespace ShopSphere.Infrastructure.Messaging;

public class RabbitMqEventPublisher : IEventPublisher, IDisposable
{
    private readonly RabbitMqOptions _options;
    private readonly IConnection _connection;
    private readonly IModel _channel;

    public RabbitMqEventPublisher(IOptions<RabbitMqOptions> options)
    {
        _options = options.Value;
        var factory = new ConnectionFactory
        {
            HostName = _options.HostName,
            UserName = _options.UserName,
            Password = _options.Password
        };
        _connection = factory.CreateConnection();
        _channel = _connection.CreateModel();
        _channel.ExchangeDeclare(exchange: _options.Exchange, type: ExchangeType.Topic, durable: true);
    }

    public Task PublishAsync(string eventName, object payload, string? messageId = null)
    {
        var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(new
        {
            eventName,
            occurredAt = DateTime.UtcNow,
            data = payload
        }));

        var props = _channel.CreateBasicProperties();
        props.MessageId = string.IsNullOrWhiteSpace(messageId) ? Guid.NewGuid().ToString() : messageId;
        props.ContentType = "application/json";

        _channel.BasicPublish(
            exchange: _options.Exchange,
            routingKey: eventName,
            basicProperties: props,
            body: body
        );

        return Task.CompletedTask;
    }

    public void Dispose()
    {
        _channel.Dispose();
        _connection.Dispose();
    }
}
