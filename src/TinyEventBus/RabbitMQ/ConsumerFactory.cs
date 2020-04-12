using System;
using System.Collections.Generic;
using System.Text;
using TinyEventBus.Factory;

namespace TinyEventBus.RabbitMQ
{
    public class ConsumerFactory : IFactory<string, IRabbitMQConnection, IConsumer>
    {
        private readonly Func<string, IRabbitMQConnection, IConsumer> _getConsumer;

        public ConsumerFactory(Func<string, IRabbitMQConnection, IConsumer> getConsumer)
        {
            this._getConsumer = getConsumer;
        }

        public IConsumer Get(string queue, IRabbitMQConnection connection) => _getConsumer(queue, connection);
    }
}
