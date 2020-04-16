using Autofac;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Polly;
using Polly.Retry;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQ.Client.Exceptions;
using TinyEventBus.Abstractions;
using TinyEventBus.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using TinyEventBus.Configuration;
using TinyEventBus.Factory;

namespace TinyEventBus.RabbitMQ
{
    public class EventBusRabbitMQ : IEventBus, IDisposable
    {
        private readonly ISubscriptionsManager _subscriptionManager;
        private readonly ILogger<EventBusRabbitMQ> _logger;
        private readonly IFactory<EventType, EventHandlerType, object> _handlersFac;
        private readonly IFactory<IConnectionStrategy> _connectionManager;
        private readonly TinyEventBusConfiguration _configuration;
        private List<IConnectionStrategy> _consumers = new List<IConnectionStrategy>();
        private IConnectionStrategy _producer;

        public EventBusRabbitMQ(ILogger<EventBusRabbitMQ> logger,
                                ISubscriptionsManager subscriptionManager,
                                IFactory<EventType, EventHandlerType, object> handlersFac,
                                IFactory<IConnectionStrategy> connectionManager,
                                TinyEventBusConfiguration configuration)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _subscriptionManager = subscriptionManager ?? throw new ArgumentNullException(nameof(subscriptionManager));
            _handlersFac = handlersFac;
            _connectionManager = connectionManager;
            _configuration = configuration;
        }

        internal void Init()
        {
            _subscriptionManager.SetOnEventRemoved(EventRemoved);
            _subscriptionManager.SetOnQueueRemoved(QueueRemoved);

            _producer = _connectionManager.Get();
            _producer.EventList = _subscriptionManager.GetProducerEvents();

            var queues = _subscriptionManager.GetConsumersQueues();
            foreach (var queue in queues)
            {
                var connection = _connectionManager.Get();
                connection.Queue = queue;
                connection.EventList = _subscriptionManager.GetConsumersEvents(queue);
                connection.ConsumerAction = messageReceived;
                connection.Start();
                _consumers.Add(connection);
            }
        }

        private void QueueRemoved(string queue)
        {
            var connection = _consumers.FirstOrDefault(c => c.Queue == queue);
            if (connection == null)
                return;

            connection.Dispose();
            _consumers.Remove(connection);
        }

        private void EventRemoved(string queue, EventType eventType)
        {
            var connection = _consumers.FirstOrDefault(c => c.Queue == queue);
            if (connection == null)
                return;

            connection.RemoveEvent(eventType);
        }

        private async Task messageReceived(string queueName, string eventName, string body)
        {
            _logger.LogTrace("Processing RabbitMQ event: {EventName}", eventName);

            var eventHandlers = _subscriptionManager.GetConsumersEvents(queueName, eventName);

            foreach (var eh in eventHandlers)
            {
                var @event = JsonConvert.DeserializeObject(body, eh.Item1.Type);
                var concreteType = typeof(IEventHandler<>).MakeGenericType(eh.Item1.Type);

                var handler = _handlersFac.Get(eh.Item1, eh.Item2);
                if (handler == null)
                    continue;

                await Task.Yield();
                await (Task)concreteType.GetMethod(nameof(IEventHandler<EventBase>.Handle)).Invoke(handler, new object[] { @event });
            }
        }

        public void Publish<T>(T @event) where T : EventBase
        {
            var eventType = new EventType(@event.GetType());
            if (_producer.EventList.Contains(eventType.Name))
            {
                _producer.Publish(@event);
            }
        }

        public void Unsubscribe<T, TH>(string queue = null)
            where T : EventBase
            where TH : IEventHandler<T>
        {
            if (queue == null)
            {
                _subscriptionManager.RemoveConsumer(new EventType(typeof(T)), new EventHandlerType(typeof(TH)));
            }
            else
            {
                _subscriptionManager.RemoveConsumer(queue, new EventType(typeof(T)), new EventHandlerType(typeof(TH)));
            }
        }

        public void Dispose()
        {
            foreach (var c in _consumers)
            {
                c.Dispose();
            }
            _consumers.Clear();
        }
    }
}
