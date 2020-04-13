using TinyEventBus.Abstractions;
using TinyEventBus.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TinyEventBus
{
    public partial class InMemorySubscriptionsManager : ISubscriptionsManager
    {
        private readonly Dictionary<string, Dictionary<EventType, List<EventHandlerType>>> _queues;
        private Action<string> _onQueueRemoved;
        private Action<string, EventType> _onEventRemoved;

        public InMemorySubscriptionsManager()
        {
            _queues = new Dictionary<string, Dictionary<EventType, List<EventHandlerType>>>();
        }

        public IEnumerable<string> GetQueues() => this._queues.Select(q => q.Key);

        public void AddSubscription(string queue, EventType eventType, EventHandlerType eventHandlerType)
        {
            if (!_queues.ContainsKey(queue))
            {
                _queues.Add(queue, new Dictionary<EventType, List<EventHandlerType>>());
            }
            var evenTypes = _queues[queue];
            if (!evenTypes.ContainsKey(eventType))
            {
                evenTypes.Add(eventType, new List<EventHandlerType>());
            }
            var eventHandlersType = evenTypes[eventType];
            if (!eventHandlersType.Contains(eventHandlerType))
            {
                eventHandlersType.Add(eventHandlerType);
            }
        }

        public void RemoveSubscriptions(string queue)
        {
            if (_queues.Remove(queue))
            {
                _onQueueRemoved?.Invoke(queue);
            }
        }

        public void RemoveSubscriptions(string queue, EventType eventType)
        {
            var evenTypes = _queues.FirstOrDefault(q => q.Key == queue && q.Value.ContainsKey(eventType));
            if (evenTypes.Key != default)
            {
                evenTypes.Value.Remove(eventType);
                _onEventRemoved?.Invoke(evenTypes.Key, eventType);
                if (evenTypes.Value.Count == 0)
                {
                    RemoveSubscriptions(queue);
                }
            }
        }

        public void RemoveSubscription(string queue, EventType eventType, EventHandlerType eventHandlerType)
        {
            var evenTypes = _queues.FirstOrDefault(q => q.Key == queue && q.Value.ContainsKey(eventType));
            if (evenTypes.Key != default)
            {
                if (!evenTypes.Value.ContainsKey(eventType))
                {
                    if (evenTypes.Value.Count == 0)
                    {
                        RemoveSubscriptions(queue);
                    }
                    return;
                }

                var eventHandlersType = evenTypes.Value[eventType];
                if (eventHandlersType.Contains(eventHandlerType))
                {
                    eventHandlersType.Remove(eventHandlerType);
                }

                if (eventHandlersType.Count == 0)
                {
                    RemoveSubscriptions(queue, eventType);
                }
            }
        }

        public void RemoveSubscriptions(EventType eventType)
        {
            var evenTypes = _queues.Where(q => q.Value.ContainsKey(eventType));
            foreach (var keys in evenTypes)
            {
                RemoveSubscriptions(keys.Key, eventType);
            }
        }

        public void RemoveSubscriptions(EventType eventType, EventHandlerType eventHandlerType)
        {
            var evenTypes = _queues.Where(q => q.Value.ContainsKey(eventType) && q.Value[eventType].Contains(eventHandlerType));
            foreach (var keys in evenTypes)
            {
                RemoveSubscription(keys.Key, eventType, eventHandlerType);
            }
        }

        public IEnumerable<string> GetEventsNameGrouped(string queueName)
        {
            return _queues.Where(q => q.Key == queueName)
                          .SelectMany(x => x.Value.Keys)
                          .GroupBy(e => e.Name)
                          .Select(q => q.Key);
        }

        public IEnumerable<EventType> GetEvents(string eventName)
        {
            return _queues.SelectMany(q => q.Value.Keys).Where(e => e.Name == eventName);
        }

        public IEnumerable<Tuple<EventType, EventHandlerType>> GetEventHandlersByEvent(string eventName = null)
        {
            var handlers = _queues.SelectMany(q => q.Value.SelectMany(e => e.Value.Select(eh => Tuple.Create(e.Key, eh))))
                                  .GroupBy(a => a.Item2)
                                  .Select(b => Tuple.Create(b.First().Item1, b.Key));

            if (!string.IsNullOrEmpty(eventName))
                handlers = handlers.Where(a => a.Item1.Name == eventName);

            return handlers;
        }

        public void SetOnQueueRemoved(Action<string> onQueueRemoved) => _onQueueRemoved = onQueueRemoved;

        public void SetOnEventRemoved(Action<string, EventType> onEventRemoved) => _onEventRemoved = onEventRemoved;

    }
}
