using System.Text.Json;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ShopSphere.Application.Messaging;
using ShopSphere.Application.Repositories;

namespace ShopSphere.Worker.Services;

public class OutboxDispatcher : BackgroundService
{
    private readonly IOutboxRepository _outboxRepository;
    private readonly IEventPublisher _publisher;
    private readonly ILogger<OutboxDispatcher> _logger;

    public OutboxDispatcher(
        IOutboxRepository outboxRepository,
        IEventPublisher publisher,
        ILogger<OutboxDispatcher> logger)
    {
        _outboxRepository = outboxRepository;
        _publisher = publisher;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            var pending = await _outboxRepository.GetPendingAsync(25, stoppingToken);
            foreach (var msg in pending)
            {
                try
                {
                    var payload = JsonSerializer.Deserialize<object>(msg.Payload) ?? new { };
                    await _publisher.PublishAsync(msg.EventName, payload, msg.Id.ToString());
                    msg.Status = "Published";
                    msg.PublishedAt = DateTime.UtcNow;
                    msg.Error = null;
                }
                catch (Exception ex)
                {
                    msg.RetryCount += 1;
                    msg.Error = ex.Message;
                    _logger.LogError(ex, "Failed to publish outbox message {MessageId}", msg.Id);
                }

                await _outboxRepository.UpdateAsync(msg, stoppingToken);
            }

            await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
        }
    }
}
