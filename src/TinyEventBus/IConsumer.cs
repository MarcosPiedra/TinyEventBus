using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace TinyEventBus.RabbitMQ
{
    public interface IConsumer : IDisposable
    {
        string QueueName { get; }
        void SetConsumerAction(Func<string, string, string, Task> consumerAction);
        void SetEvents(IEnumerable<EventType> events);
        void StartConsuming();
        void RemoveEvent(EventType eventType);
    }
}
