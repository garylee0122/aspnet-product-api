using DemoAPI.Data;
using DemoAPI.Enums;
using DemoAPI.Infrastructure.Queues;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace DemoAPI.Infrastructure.Workers
{
    public class OrderWorker : BackgroundService
    {
        private readonly OrderQueue _queue;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<OrderWorker> _logger;

        public OrderWorker(OrderQueue queue, IServiceScopeFactory scopeFactory, ILogger<OrderWorker> logger)
        {
            _queue = queue;
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            Console.WriteLine("OrderWorker started...");
            _logger.LogInformation("OrderWorker started...");

            while (!stoppingToken.IsCancellationRequested)
            {
                if (_queue.TryDequeue(out var queueItem) && queueItem != null)
                {
                    try
                    {
                        Console.WriteLine($"Processing order {queueItem.OrderId}");
                        _logger.LogInformation("Processing order {OrderId}", queueItem.OrderId);

                        if (new Random().Next(2) == 0)
                        {
                            throw new Exception("Random failure");
                        }

                        await Task.Delay(3000, stoppingToken);
                        await UpdateOrderStatusAsync(queueItem.OrderId, OrderStatus.Created, stoppingToken);

                        Console.WriteLine("Processing order {0} Success!", queueItem.OrderId);
                        _logger.LogInformation("Processing order {OrderId} Success!", queueItem.OrderId);
                    }
                    catch (Exception ex)
                    {
                        queueItem.RetryCount++;

                        if (queueItem.RetryCount < 3)
                        {
                            Console.WriteLine($"Retry {queueItem.RetryCount} for order {queueItem.OrderId}");
                            _logger.LogError($"Retry {queueItem.RetryCount} for order {queueItem.OrderId}");

                            var delay = (int)Math.Pow(2, queueItem.RetryCount) * 1000;
                            await Task.Delay(delay, stoppingToken);
                            _queue.Enqueue(queueItem);
                        }
                        else
                        {
                            await UpdateOrderStatusAsync(queueItem.OrderId, OrderStatus.Failed, stoppingToken);

                            Console.WriteLine($"Order {queueItem.OrderId} failed permanently");
                            _logger.LogError($"Order {queueItem.OrderId} failed permanently");
                        }

                        Console.WriteLine($"Failed to process order {queueItem.OrderId}: {ex.Message}");
                        _logger.LogError(ex, "Failed to process order {OrderId}", queueItem.OrderId);
                    }
                }

                await Task.Delay(1000, stoppingToken);
            }

            Console.WriteLine("OrderWorker stopped.");
            _logger.LogInformation("OrderWorker stopped.");
        }

        private async Task UpdateOrderStatusAsync(int orderId, OrderStatus status, CancellationToken cancellationToken)
        {
            using var scope = _scopeFactory.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            var order = await context.Orders.FindAsync(orderId);
            if (order == null)
            {
                return;
            }

            order.Status = status;
            await context.SaveChangesAsync(cancellationToken);
        }
    }
}
