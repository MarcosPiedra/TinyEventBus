using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using TinyEventBus.Configuration;

namespace TinyEventBus.RabbitMQ
{
    public class Consumer : IConsumer
    {
        private readonly IRabbitMQConnection _connection;
        private readonly TinyEventBusConfiguration _configuration;
        private readonly ILogger<Consumer> _logger;
        private IModel _channel;
        private Func<string, string, string, Task> _consumerAction;
        private IEnumerable<EventType> _events;

        public Consumer(string queueName,
                        IRabbitMQConnection connection,
                        TinyEventBusConfiguration configuration,
                        ILogger<Consumer> logger)
        {
            this._connection = connection ?? throw new ArgumentNullException(nameof(connection));
            this._configuration = configuration;
            this._logger = logger;
            this.QueueName = queueName;
        }

        public string QueueName { get; internal set; }

        public void RemoveEvent(EventType eventType)
        {
            this._channel.QueueUnbind(queue: this.QueueName, exchange: this._configuration.ExchangeName, routingKey: eventType.Name);
        }

        public void SetConsumerAction(Func<string, string, string, Task> consumerAction) => this._consumerAction = consumerAction;

        public void SetEvents(IEnumerable<EventType> events) => this._events = events;

        public void StartConsuming()
        {
            this._logger.LogTrace("Starting RabbitMQ basic consume");

            this._channel = _connection.CreateModel();
            this._channel.ExchangeDeclare(exchange: _configuration.ExchangeName, type: "direct");
            this._channel.QueueDeclare(queue: this.QueueName, durable: true, exclusive: false, autoDelete: false, arguments: null);

            foreach (var @event in this._events)
            {
                this._channel.QueueBind(queue: this.QueueName, exchange: this._configuration.ExchangeName, routingKey: @event.Name);
            }

            var consumer = new AsyncEventingBasicConsumer(_channel);
            consumer.Received += Consumer_Received;
            this._channel.BasicConsume(queue: this.QueueName, autoAck: false, consumer: consumer);
        }

        private Task Consumer_Received(object sender, BasicDeliverEventArgs @event)
        {
            var eventName = @event.RoutingKey;
            var message = Encoding.UTF8.GetString(@event.Body);
            var task = this._consumerAction?.Invoke(this.QueueName, eventName, message);
            this._channel.BasicAck(@event.DeliveryTag, multiple: false);
            return task;
        }

        public void Dispose()
        {
            this._channel.Close();
            this._channel.Dispose();
        }
    }
}
