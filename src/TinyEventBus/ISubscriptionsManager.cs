using TinyEventBus.Events;
using System;
using System.Collections.Generic;
using TinyEventBus.Abstractions;
using System.Threading.Tasks;

namespace TinyEventBus
{
    public interface ISubscriptionsManager
    {
        IEnumerable<string> GetConsumersQueues();
        void AddOrUpdateConsumer(string queue, EventType eventType, EventHandlerType eventHandlerType);
        void AddOrUpdateProducer(EventType eventType);
        void SetOnQueueRemoved(Action<string> onRemoveQueue);
        void SetOnEventRemoved(Action<string, EventType> onEventRemoved);
        void RemoveConsumer(string queue);
        void RemoveConsumer(string queue, EventType eventType);
        void RemoveConsumer(string queue, EventType eventType, EventHandlerType eventHandlerType);
        void RemoveConsumer(EventType eventType);
        void RemoveConsumer(EventType eventType, EventHandlerType eventHandlerType);
        IEnumerable<string> GetConsumersEvents(string queue);
        IEnumerable<Tuple<EventType, EventHandlerType>> GetConsumersEvents(string queueName = null, string eventName = null);
        IEnumerable<string> GetProducerEvents();
    }
}