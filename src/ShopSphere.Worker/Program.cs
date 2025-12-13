using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using ShopSphere.Application.Caching;
using ShopSphere.Application.Messaging;
using ShopSphere.Application.Repositories;
using ShopSphere.Application.Services;
using ShopSphere.Infrastructure.Messaging;
using ShopSphere.Infrastructure.Repositories;
using ShopSphere.Infrastructure.db;
using ShopSphere.Worker.Services;

var host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((context, services) =>
    {
        services.Configure<RabbitMqOptions>(context.Configuration.GetSection("RabbitMq"));
        services.Configure<CacheOptions>(context.Configuration.GetSection("Cache"));

        services.AddDbContext<ShopSphereDbContext>(options =>
            options.UseSqlServer(context.Configuration.GetConnectionString("Default"))
        );

        services.AddScoped<IOutboxRepository, OutboxRepository>();
        services.AddScoped<IProcessedMessageRepository, ProcessedMessageRepository>();
        services.AddScoped<IOrderRepository, OrderRepository>();
        services.AddScoped<IPaymentRepository, PaymentRepository>();
        services.AddScoped<IEventPublisher, RabbitMqEventPublisher>();
        services.AddScoped<CheckoutService>();

        services.AddHostedService<OutboxDispatcher>();
        services.AddHostedService<OrderPaymentConsumer>();
    })
    .Build();

await host.RunAsync();
