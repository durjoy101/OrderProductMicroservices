using Confluent.Kafka;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Hosting;
using ProductService.Application.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace ProductService.Infrastructure.Messaging
{
    public class KafkaOrderPlacedConsumer : BackgroundService, IOrderPlacedConsumer
    {
        private readonly ILogger<KafkaOrderPlacedConsumer> _logger;
        private readonly IProductRepository _repo;
        private readonly ICacheService _cache;
        private readonly ConsumerConfig _config;
        private readonly string _topic;
        private IConsumer<Ignore, string>? _consumer;

        public KafkaOrderPlacedConsumer(
            ILogger<KafkaOrderPlacedConsumer> logger,
            IProductRepository repo,
            ICacheService cache,
            IOptions<KafkaSettings> options)
        {
            _logger = logger;
            _repo = repo;
            _cache = cache;
            _config = new ConsumerConfig
            {
                BootstrapServers = options.Value.BootstrapServers,
                GroupId = options.Value.GroupId,
                AutoOffsetReset = AutoOffsetReset.Earliest,
                EnableAutoCommit = true
            };
            _topic = options.Value.OrderPlacedTopic;
        }

        // Called by host
        public void Start() => _ = StartAsync(default);

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _consumer = new ConsumerBuilder<Ignore, string>(_config).Build();
            _consumer.Subscribe(_topic);

            return Task.Run(async () =>
            {
                while (!stoppingToken.IsCancellationRequested)
                {
                    try
                    {
                        var cr = _consumer.Consume(stoppingToken);
                        if (cr is null) continue;

                        // Expected payload: { "productId":"...", "quantity": 2 }
                        var payload = JsonSerializer.Deserialize<OrderPlacedMessage>(cr.Message.Value);
                        if (payload is not null)
                        {
                            var ok = await _repo.UpdateStockAsync(payload.productId, -payload.quantity, stoppingToken);

                            // Invalidate product list cache when stock changes
                            if (ok) await _cache.RemoveAsync("products:all", stoppingToken);
                        }
                    }
                    catch (OperationCanceledException) { /* normal on shutdown */ }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Kafka consumption error");
                        await Task.Delay(1000, stoppingToken);
                    }
                }
            }, stoppingToken);
        }

        public override Task StopAsync(CancellationToken cancellationToken)
        {
            _consumer?.Close();
            _consumer?.Dispose();
            return base.StopAsync(cancellationToken);
        }

        private record OrderPlacedMessage(string productId, int quantity);
    }

    public class KafkaSettings
    {
        public string BootstrapServers { get; set; } = "kafka:9092";
        public string GroupId { get; set; } = "product-service";
        public string OrderPlacedTopic { get; set; } = "order-placed";
    }
}
