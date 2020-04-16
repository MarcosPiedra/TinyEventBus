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
    public class PubSub : IConnectionStrategy
    {
        private readonly IRabbitMQConnection _connection;
        private readonly ILogger<PubSub> _logger;
        private readonly TinyEventBusConfiguration _configuration;
        private readonly string _exchangeType;
        private IModel _channel;

        public PubSub(IRabbitMQConnection persistentConnection,
                      ILogger<PubSub> logger,
                      TinyEventBusConfiguration configuration)
        {
            _connection = persistentConnection ?? throw new ArgumentNullException(nameof(persistentConnection));
            _logger = logger;
            _configuration = configuration;

            _exchangeType = configuration.ExchangeType;
            if (string.IsNullOrEmpty(_exchangeType))
                _exchangeType = ExchangeType.Direct;
        }

        public string Queue { get; set; } = "";
        public IEnumerable<string> EventList { get; set; } 
        public Func<string, string, string, Task> ConsumerAction { get; set; } = null;

        public void Publish<T>(T @event) where T : EventBase
        {
            if (!_connection.IsConnected)
            {
                _connection.TryConnect();
            }

            var eventType = new EventType(@event.GetType());

            _logger.LogTrace("Creating RabbitMQ channel to publish event: {EventId} ({EventName})", @event.Id, eventType.Name);

            _logger.LogTrace("Declaring RabbitMQ exchange to publish event: {EventId}", @event.Id);

            _channel = _channel ?? _connection.CreateModel();
            _channel.ExchangeDeclare(exchange: _configuration.ExchangeName, type: _exchangeType);

            var message = JsonConvert.SerializeObject(@event);
            var body = Encoding.UTF8.GetBytes(message);

            Policy.Handle<BrokerUnreachableException>()
                  .Or<SocketException>()
                  .WaitAndRetry(_configuration.Retries.Publish, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)), (ex, time) =>
                  {
                      _logger.LogWarning(ex, "Could not publish event: {EventId} after {Timeout}s ({ExceptionMessage})", @event.Id, $"{time.TotalSeconds:n1}", ex.Message);
                  })
                  .Execute(() =>
                  {
                      var properties = _channel.CreateBasicProperties();
                      properties.Persistent = true;

                      _logger.LogTrace("Publishing event to RabbitMQ: {EventId}", @event.Id);

                      _channel.BasicPublish(exchange: _configuration.ExchangeName, routingKey: eventType.Name, mandatory: true, basicProperties: properties, body: body);
                  });
        }

        public void RemoveEvent(EventType eventType)
        {
            _channel.QueueUnbind(queue: Queue, exchange: _configuration.ExchangeName, routingKey: eventType.Name);
        }

        public void Start()
        {
            if (!_connection.IsConnected)
            {
                _connection.TryConnect();
            }

            _logger.LogTrace("Starting RabbitMQ basic consume");

            _channel = _channel ?? _connection.CreateModel();
            _channel.ExchangeDeclare(exchange: _configuration.ExchangeName, type: _exchangeType);
            _channel.QueueDeclare(queue: Queue, durable: true, exclusive: false, autoDelete: false, arguments: null);

            foreach (var @event in EventList)
            {
                _channel.QueueBind(queue: Queue, exchange: _configuration.ExchangeName, routingKey: @event);
            }

            var consumer = new AsyncEventingBasicConsumer(_channel);
            consumer.Received += Consumer_Received;
            _channel.BasicConsume(queue: Queue, autoAck: false, consumer: consumer);
        }

        private Task Consumer_Received(object sender, BasicDeliverEventArgs @event)
        {
            var eventName = @event.RoutingKey;
            var message = Encoding.UTF8.GetString(@event.Body);
            var task = ConsumerAction?.Invoke(Queue, eventName, message);
            _channel.BasicAck(@event.DeliveryTag, multiple: false);
            return task;
        }

        public void Dispose()
        {
            _channel.Close();
            _channel.Dispose();
        }
    }
}
