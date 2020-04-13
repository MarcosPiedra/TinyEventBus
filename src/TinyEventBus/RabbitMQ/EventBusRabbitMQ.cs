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
        private List<IConnectionStrategy> _connections = new List<IConnectionStrategy>();

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

            var queues = _subscriptionManager.GetQueues();
            foreach (var queue in queues)
            {
                var connection = _connectionManager.Get();

                connection.Start(new StartParams()
                {
                    QueueName = queue,
                    EventList = _subscriptionManager.GetEventsNameGrouped(queue),
                    ConsumerAction = messageReceived,
                });

                _connections.Add(connection);
            }
        }

        private void QueueRemoved(string queue)
        {
            var connection = _connections.FirstOrDefault(c => c.Queue == queue);
            if (connection == null)
                return;

            connection.Dispose();
            _connections.Remove(connection);
        }

        private void EventRemoved(string queue, EventType eventType)
        {
            var connection = _connections.FirstOrDefault(c => c.Queue == queue);
            if (connection == null)
                return;

            connection.RemoveEvent(eventType);
        }

        private async Task messageReceived(string queueName, string eventName, string body)
        {
            _logger.LogTrace("Processing RabbitMQ event: {EventName}", eventName);

            var eventHandlers = _subscriptionManager.GetEventHandlersByEvent(eventName);

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
            foreach (var connection in _connections)
            {
                var eventType = new EventType(@event.GetType());
                if (connection.EventList.Contains(eventType.Name))
                {
                    connection.Publish(@event);
                }
            }
        }

        public void Unsubscribe<T, TH>(string queue = null)
            where T : EventBase
            where TH : IEventHandler<T>
        {
            if (queue == null)
            {
                this._subscriptionManager.RemoveSubscriptions(new EventType(typeof(T)), new EventHandlerType(typeof(TH)));
            }
            else
            {
                this._subscriptionManager.RemoveSubscription(queue, new EventType(typeof(T)), new EventHandlerType(typeof(TH)));
            }
        }

        public void Dispose()
        {
            foreach (var c in _connections)
            {
                c.Dispose();
            }
            _connections.Clear();
        }
    }
}
