using TinyEventBus.Abstractions;
using TinyEventBus.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TinyEventBus
{
    public class InMemorySubscriptionsManager : ISubscriptionsManager
    {
        private readonly Dictionary<string, Dictionary<EventType, List<EventHandlerType>>> _consumer;
        private readonly List<EventType> _producer;
        private Action<string> _onQueueRemoved;
        private Action<string, EventType> _onEventRemoved;

        public InMemorySubscriptionsManager()
        {
            _consumer = new Dictionary<string, Dictionary<EventType, List<EventHandlerType>>>();
            _producer = new List<EventType>();
        }

        public void AddOrUpdateConsumer(string queue, EventType eventType, EventHandlerType eventHandlerType)
        {
            if (!_consumer.ContainsKey(queue))
            {
                _consumer.Add(queue, new Dictionary<EventType, List<EventHandlerType>>());
            }
            var evenTypes = _consumer[queue];
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

        public void AddOrUpdateProducer(EventType eventType)
        {
            if (!_producer.Contains(eventType))
            {
                _producer.Add(eventType);
            }
        }

        public void SetOnQueueRemoved(Action<string> onQueueRemoved) => _onQueueRemoved = onQueueRemoved;

        public void SetOnEventRemoved(Action<string, EventType> onEventRemoved) => _onEventRemoved = onEventRemoved;

        public void RemoveConsumer(string queue)
        {
            if (_consumer.Remove(queue))
            {
                _onQueueRemoved?.Invoke(queue);
            }
        }

        public void RemoveConsumer(string queue, EventType eventType)
        {
            var evenTypes = _consumer.FirstOrDefault(q => q.Key == queue && q.Value.ContainsKey(eventType));
            if (evenTypes.Key != default)
            {
                evenTypes.Value.Remove(eventType);
                _onEventRemoved?.Invoke(evenTypes.Key, eventType);
                if (evenTypes.Value.Count == 0)
                {
                    RemoveConsumer(queue);
                }
            }
        }

        public void RemoveConsumer(string queue, EventType eventType, EventHandlerType eventHandlerType)
        {
            var evenTypes = _consumer.FirstOrDefault(q => q.Key == queue && q.Value.ContainsKey(eventType));
            if (evenTypes.Key != default)
            {
                if (!evenTypes.Value.ContainsKey(eventType))
                {
                    if (evenTypes.Value.Count == 0)
                    {
                        RemoveConsumer(queue);
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
                    RemoveConsumer(queue, eventType);
                }
            }
        }

        public void RemoveConsumer(EventType eventType)
        {
            var evenTypes = _consumer.Where(q => q.Value.ContainsKey(eventType));
            foreach (var keys in evenTypes)
            {
                RemoveConsumer(keys.Key, eventType);
            }
        }

        public void RemoveConsumer(EventType eventType, EventHandlerType eventHandlerType)
        {
            var evenTypes = _consumer.Where(q => q.Value.ContainsKey(eventType) && q.Value[eventType].Contains(eventHandlerType));
            foreach (var keys in evenTypes)
            {
                RemoveConsumer(keys.Key, eventType, eventHandlerType);
            }
        }

        public IEnumerable<string> GetConsumersQueues() => _consumer.Select(q => q.Key).AsEnumerable();

        public IEnumerable<string> GetConsumersEvents(string queueName)
        {
            return _consumer.Where(q => q.Key == queueName)
                            .SelectMany(x => x.Value.Keys)
                            .GroupBy(e => e.Name)
                            .Select(q => q.Key)
                            .AsEnumerable();
        }

        public IEnumerable<string> GetProducerEvents()
        {
            return _producer.GroupBy(e => e.Name)
                            .Select(q => q.Key)
                            .AsEnumerable();
        }

        public IEnumerable<Tuple<EventType, EventHandlerType>> GetConsumersEvents(string queueName = null, string eventName = null)
        {
            var queues = _consumer.AsQueryable();

            if (!string.IsNullOrEmpty(queueName))
                queues = queues.Where(q => q.Key == queueName);

            var handlers = queues.SelectMany(q => q.Value.SelectMany(e => e.Value.Select(eh => Tuple.Create(e.Key, eh))));
            //.GroupBy(a => a.Item2)
            //.Select(b => Tuple.Create(b.First().Item1, b.Key));

            if (!string.IsNullOrEmpty(eventName))
                handlers = handlers.Where(a => a.Item1.Name == eventName);

            return handlers.AsEnumerable();
        }
    }
}
