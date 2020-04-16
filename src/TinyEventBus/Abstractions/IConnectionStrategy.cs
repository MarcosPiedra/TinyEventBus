using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using TinyEventBus.Events;

namespace TinyEventBus.Abstractions
{
    public interface IConnectionStrategy : IDisposable
    {
        IEnumerable<string> EventList { get; set;  }
        string Queue { get; set; }
        Func<string, string, string, Task> ConsumerAction { get; set; }
        void Start();
        void RemoveEvent(EventType eventType);
        void Publish<T>(T eventType) where T : EventBase;
    }
}
