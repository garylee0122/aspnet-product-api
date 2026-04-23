using Microsoft.Extensions.Hosting;
using DemoAPI.Infrastructure.Queues;
using Microsoft.Extensions.Logging;

namespace DemoAPI.Infrastructure.Workers
{
    public class OrderWorker : BackgroundService
    {
        private readonly OrderQueue _queue;
        private readonly ILogger<OrderWorker> _logger;

        public OrderWorker(OrderQueue queue, ILogger<OrderWorker> logger)
        {
            _queue = queue;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            Console.WriteLine("OrderWorker started...");
            _logger.LogInformation("OrderWorker started...");
            while (!stoppingToken.IsCancellationRequested)
            {
                if (_queue.TryDequeue(out int orderId))
                {
                    Console.WriteLine($"Processing order {orderId}");
                    _logger.LogInformation($"Processing order {orderId}");

                    await Task.Delay(3000); // 模擬工作
                }

                await Task.Delay(1000);
            }
            Console.WriteLine("OrderWorker stopped.");
            _logger.LogInformation("OrderWorker stopped.");
        }
    }
}