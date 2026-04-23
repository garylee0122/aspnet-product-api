using System.Collections.Concurrent;

namespace DemoAPI.Infrastructure.Queues
{
    public class OrderQueue
    {
        private readonly ConcurrentQueue<int> _queue = new();

        public void Enqueue(int orderId)
        {
            _queue.Enqueue(orderId);
        }

        public bool TryDequeue(out int orderId)
        {
            return _queue.TryDequeue(out orderId);
        }
    }
}