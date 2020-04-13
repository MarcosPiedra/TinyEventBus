using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Polly;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQ.Client.Exceptions;
using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using TinyEventBus.Abstractions;
using TinyEventBus.Configuration;
using TinyEventBus.Events;

namespace TinyEventBus.RabbitMQ.Connections
{
    public class WorkQueue : IConnectionStrategy
    {
        private readonly IRabbitMQConnection _connection;
        private readonly ILogger<WorkQueue> _logger;
        private readonly TinyEventBusConfiguration _configuration;
        private Func<string, string, string, Task> _consumerAction;
        private IModel _channel;

        public WorkQueue(IRabbitMQConnection persistentConnection,
                         ILogger<WorkQueue> logger,
                         TinyEventBusConfiguration configuration)
        {
            _connection = persistentConnection ?? throw new ArgumentNullException(nameof(persistentConnection));
            _logger = logger;
            _configuration = configuration;
        }

        public string Queue { get; private set; } = "";
        public IEnumerable<string> EventList { get; private set; }

        public void Publish<T>(T @event) where T : EventBase
        {
            if (!_connection.IsConnected)
            {
                _connection.TryConnect();
            }

            var eventType = new EventType(@event.GetType());

            _logger.LogTrace("Creating RabbitMQ channel to publish event: {EventId} ({EventName})", @event.Id, eventType.Name);

            _channel = _channel ?? _connection.CreateModel();
            if (!string.IsNullOrEmpty(Queue))
            {
                _logger.LogTrace("Declaring RabbitMQ queue to publish event: {EventId}", @event.Id);
                _channel.QueueDeclare(queue: Queue, durable: true, exclusive: false, autoDelete: false, arguments: null);
            }

            var xx = JsonConvert.SerializeObject(@event);
            var message = JsonConvert.SerializeObject(new Message() { EventContent = xx, EventName = eventType.Name });
            var body = Encoding.UTF8.GetBytes(message);

            Policy.Handle<BrokerUnreachableException>()
                  .Or<SocketException>()
                  .WaitAndRetry(_configuration.Retries.Publish, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)), (ex, time) =>
                  {
                      _logger.LogWarning(ex, "Could not publish event: {EventId} after {Timeout}s ({ExceptionMessage})", @event.Id, $"{time.TotalSeconds:n1}", ex.Message);
                  }).Execute(() =>
                  {
                      var properties = _channel.CreateBasicProperties();
                      properties.Persistent = true; 

                      _logger.LogTrace("Publishing event to RabbitMQ: {EventId}", @event.Id);

                      _channel.BasicPublish(exchange: "", routingKey: Queue, basicProperties: properties, body: body);
                  });
        }

        public void RemoveEvent(EventType eventType)
        {

        }

        public void Start(StartParams @params)
        {
            if (!_connection.IsConnected)
            {
                _connection.TryConnect();
            }

            _consumerAction = @params.ConsumerAction;
            Queue = @params.QueueName;
            EventList = @params.EventList;

            _logger.LogTrace("Starting RabbitMQ basic consume");

            _channel = _channel ?? _connection.CreateModel();

            _channel.QueueDeclare(queue: Queue, durable: true, exclusive: false, autoDelete: false, arguments: null);
            _channel.BasicQos(prefetchSize: 0, prefetchCount: 1, global: false);

            var consumer = new AsyncEventingBasicConsumer(_channel);
            consumer.Received += Consumer_Received;
            _channel.BasicConsume(queue: Queue, autoAck: false, consumer: consumer);
        }

        private Task Consumer_Received(object sender, BasicDeliverEventArgs @event)
        {
            var message = JsonConvert.DeserializeObject<Message>(Encoding.UTF8.GetString(@event.Body));
            var task = _consumerAction?.Invoke(Queue, message.EventName, message.EventContent);
            _channel.BasicAck(@event.DeliveryTag, multiple: false);
            return task;
        }

        public void Dispose()
        {
            _channel.Close();
            _channel.Dispose();
        }

        public class Message
        {
            public string EventContent { get; set; } = "";
            public string EventName { get; set; } = "";
        }
    }
}
