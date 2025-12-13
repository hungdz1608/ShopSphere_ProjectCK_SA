using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using ShopSphere.Application.Messaging;
using ShopSphere.Application.Repositories;
using ShopSphere.Application.Services;
using ShopSphere.Domain.Entities;

namespace ShopSphere.Worker.Services;

public class OrderPaymentConsumer : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly RabbitMqOptions _options;
    private readonly ILogger<OrderPaymentConsumer> _logger;
    private IConnection? _connection;
    private IModel? _channel;

    public OrderPaymentConsumer(
        IServiceScopeFactory scopeFactory,
        IOptions<RabbitMqOptions> options,
        ILogger<OrderPaymentConsumer> logger)
    {
        _scopeFactory = scopeFactory;
        _options = options.Value;
        _logger = logger;
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var factory = new ConnectionFactory
        {
            HostName = _options.HostName,
            UserName = _options.UserName,
            Password = _options.Password
        };

        _connection = factory.CreateConnection();
        _channel = _connection.CreateModel();
        _channel.ExchangeDeclare(_options.Exchange, ExchangeType.Topic, durable: true);

        var queueName = _channel.QueueDeclare().QueueName;
        _channel.QueueBind(queueName, _options.Exchange, routingKey: "order.*");

        var consumer = new AsyncEventingBasicConsumer(_channel);
        consumer.Received += async (_, ea) =>
        {
            var messageId = string.IsNullOrWhiteSpace(ea.BasicProperties.MessageId)
                ? Guid.NewGuid().ToString()
                : ea.BasicProperties.MessageId;

            using var scope = _scopeFactory.CreateScope();
            var processedRepo = scope.ServiceProvider.GetRequiredService<IProcessedMessageRepository>();
            if (await processedRepo.ExistsAsync(messageId, stoppingToken))
            {
                _channel.BasicAck(ea.DeliveryTag, multiple: false);
                return;
            }

            try
            {
                var payloadJson = Encoding.UTF8.GetString(ea.Body.ToArray());
                var root = JsonDocument.Parse(payloadJson).RootElement;
                var data = root.GetProperty("data");
                var orderId = data.GetProperty("id").GetGuid();

                var checkout = scope.ServiceProvider.GetRequiredService<CheckoutService>();
                await checkout.MarkPaymentCompletedAsync(orderId, stoppingToken);

                await processedRepo.AddAsync(new ProcessedMessage
                {
                    MessageId = messageId,
                    ProcessedAt = DateTime.UtcNow
                }, stoppingToken);

                _channel.BasicAck(ea.DeliveryTag, multiple: false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to handle message {MessageId}", messageId);
                _channel.BasicNack(ea.DeliveryTag, multiple: false, requeue: false);
            }
        };

        _channel.BasicConsume(queue: queueName, autoAck: false, consumer: consumer);
        return Task.CompletedTask;
    }

    public override void Dispose()
    {
        _channel?.Dispose();
        _connection?.Dispose();
        base.Dispose();
    }
}
