using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using TinyEventBus.Events;

namespace TinyEventBus.Abstractions
{
    public interface IConnectionStrategy : IDisposable
    {
        IEnumerable<string> EventList { get; }
        string Queue { get; }
        void Start(StartParams @params);
        void RemoveEvent(EventType eventType);
        void Publish<T>(T eventType) where T : EventBase;
    }

    public class StartParams
    {
        public string QueueName { get; set; }
        public IEnumerable<string> EventList { get; set; } = new List<string>();
        public Func<string, string, string, Task> ConsumerAction { get; set; } = null;
    }
}
