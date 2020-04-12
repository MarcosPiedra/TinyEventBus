using TinyEventBus.Events;
using System;

namespace TinyEventBus.Abstractions
{
    public interface IEventBus
    {
        void Publish<T>(T @event) where T : EventBase;
        void Unsubscribe<T, TH>(string queue = null) where TH : IEventHandler<T> where T : EventBase;
    }
}
