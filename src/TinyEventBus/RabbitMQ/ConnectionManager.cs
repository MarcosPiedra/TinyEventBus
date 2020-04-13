using System;
using System.Collections.Generic;
using System.Text;
using TinyEventBus.Abstractions;
using TinyEventBus.Factory;

namespace TinyEventBus.RabbitMQ
{
    public class ConnectionManager : IFactory<IConnectionStrategy>
    {
        private readonly Func<IConnectionStrategy> _getConnection;

        public ConnectionManager(Func<IConnectionStrategy> getConnection)
        {
            _getConnection = getConnection;
        }

        public IConnectionStrategy Get() => _getConnection();
    }
}
