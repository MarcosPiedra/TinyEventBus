using TinyEventBus.Events;
using System;
using System.Collections.Generic;
using TinyEventBus.Abstractions;
using System.Threading.Tasks;

namespace TinyEventBus
{
    public interface ISubscriptionsManager
    {
        IEnumerable<string> GetQueues();
        void SetOnQueueRemoved(Action<string> onRemoveQueue);
        void SetOnEventRemoved(Action<string, EventType> onEventRemoved);
        void AddSubscription(string queue, EventType eventType, EventHandlerType eventHandlerType);
        void RemoveSubscriptions(string queue);
        void RemoveSubscriptions(string queue, EventType eventType);
        void RemoveSubscription(string queue, EventType eventType, EventHandlerType eventHandlerType);
        void RemoveSubscriptions(EventType eventType);
        void RemoveSubscriptions(EventType eventType, EventHandlerType eventHandlerType);
        IEnumerable<string> GetEventsNameGrouped(string queue);
        IEnumerable<Tuple<EventType, EventHandlerType>> GetEventHandlersByEvent(string @eventName = null);
        IEnumerable<EventType> GetEvents(string eventName);
    }
}