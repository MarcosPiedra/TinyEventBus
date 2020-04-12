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
        private readonly IRabbitMQConnection _persistentConnection;
        private readonly ILogger<EventBusRabbitMQ> _logger;
        private readonly ISubscriptionsManager _subscriptionManager;
        private readonly IFactory<EventType, EventHandlerType, object> _handlersFac;
        private readonly IFactory<string, IRabbitMQConnection, IConsumer> _consumerFac;
        private readonly TinyEventBusConfiguration _configuration;
        private List<IConsumer> consumers = new List<IConsumer>();

        public EventBusRabbitMQ(IRabbitMQConnection persistentConnection,
                                ILogger<EventBusRabbitMQ> logger,
                                ISubscriptionsManager subscriptionManager,
                                IFactory<EventType, EventHandlerType, object> handlersFac,
                                IFactory<string, IRabbitMQConnection, IConsumer> consumersFac,
                                TinyEventBusConfiguration configuration)
        {
            _persistentConnection = persistentConnection ?? throw new ArgumentNullException(nameof(persistentConnection));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _subscriptionManager = subscriptionManager ?? throw new ArgumentNullException(nameof(subscriptionManager));
            _handlersFac = handlersFac;
            _consumerFac = consumersFac;
            _configuration = configuration;
        }

        internal void Init()
        {
            if (!_persistentConnection.IsConnected)
            {
                _persistentConnection.TryConnect();
            }

            _subscriptionManager.SetOnEventRemoved(EventRemoved);
            _subscriptionManager.SetOnQueueRemoved(QueueRemoved);

            var queues = _subscriptionManager.GetQueues();
            foreach (var queue in queues)
            {
                var events = _subscriptionManager.GetEvents(queue);
                var consumer = _consumerFac.Get(queue, this._persistentConnection);
                consumer.SetConsumerAction(messageReceived);
                consumer.SetEvents(events);
                consumers.Add(consumer);
            }

            foreach (var c in consumers)
            {
                c.StartConsuming();
            }
        }

        private void QueueRemoved(string queue)
        {
            var consumer = consumers.FirstOrDefault(c => c.QueueName == queue);
            if (consumer == null)
                return;

            consumer.Dispose();
            consumers.Remove(consumer);
        }

        private void EventRemoved(string queue, EventType eventType)
        {
            var consumer = consumers.FirstOrDefault(c => c.QueueName == queue);
            if (consumer == null)
                return;

            consumer.RemoveEvent(eventType);
        }

        private async Task messageReceived(string queueName, string eventName, string body)
        {
            _logger.LogTrace("Processing RabbitMQ event: {EventName}", eventName);

            var eventHandlers = _subscriptionManager.GetEventHandlersByEvents(eventName);
            var eventType = _subscriptionManager.GetEvent(eventName);

            foreach (var e in eventHandlers)
            {
                var @event = JsonConvert.DeserializeObject(body, eventType.Type);
                var concreteType = typeof(IEventHandler<>).MakeGenericType(eventType.Type);

                var handler = _handlersFac.Get(eventType, e);
                if (handler == null)
                    continue;

                await Task.Yield();
                await (Task)concreteType.GetMethod(nameof(IEventHandler<EventBase>.Handle)).Invoke(handler, new object[] { @event });
            }
        }

        public void Publish<T>(T @event) where T : EventBase
        {
            if (!_persistentConnection.IsConnected)
            {
                _persistentConnection.TryConnect();
            }

            var eventType = new EventType(@event.GetType());

            var policy = Policy.Handle<BrokerUnreachableException>()
                .Or<SocketException>()
                .WaitAndRetry(this._configuration.Retries.Publish, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)), (ex, time) =>
                {
                    _logger.LogWarning(ex, "Could not publish event: {EventId} after {Timeout}s ({ExceptionMessage})", @event.Id, $"{time.TotalSeconds:n1}", ex.Message);
                });

            _logger.LogTrace("Creating RabbitMQ channel to publish event: {EventId} ({EventName})", @event.Id, eventType.Name);

            using (var channel = _persistentConnection.CreateModel())
            {
                _logger.LogTrace("Declaring RabbitMQ exchange to publish event: {EventId}", @event.Id);

                channel.ExchangeDeclare(exchange: this._configuration.ExchangeName, type: "direct");

                var message = JsonConvert.SerializeObject(@event);
                var body = Encoding.UTF8.GetBytes(message);

                policy.Execute(() =>
                {
                    var properties = channel.CreateBasicProperties();
                    properties.DeliveryMode = 2; // persistent

                    _logger.LogTrace("Publishing event to RabbitMQ: {EventId}", @event.Id);

                    channel.BasicPublish(
                        exchange: this._configuration.ExchangeName,                        
                        routingKey: eventType.Name,
                        mandatory: true,
                        basicProperties: properties,
                        body: body);
                });
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
            foreach (var c in consumers)
            {
                c.Dispose();
            }
            consumers.Clear();
        }
    }
}
