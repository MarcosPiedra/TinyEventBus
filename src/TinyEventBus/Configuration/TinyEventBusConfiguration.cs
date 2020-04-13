using System;
using System.Collections.Generic;
using System.Text;

namespace TinyEventBus.Configuration
{
    public class TinyEventBusConfiguration
    {
        public string BrokerName { get; set; } = "";
        public string ExchangeName { get; set; } = "";
        public string ExchangeType { get; set; } = "";
        public ConnectionStrategy ConnectionStrategy { get; set; } = ConnectionStrategy.PubSub;
        public RetriesConfiguration Retries { get; set; } = new RetriesConfiguration();
        public string Hostname { get; set; } = "";
        public string Username { get; set; } = "";
        public string Password { get; set; } = "";
        public int Port { get; set; } = 0;
        public string VirtualHost { get; set; } = "";
        public List<QueueConfiguration> Queues { get; set; } = new List<QueueConfiguration>();
    }

    public enum ConnectionStrategy
    {
        PubSub = 0,
        WorkQueue = 1,
    }

    public class RetriesConfiguration
    {
        public int Connection { get; set; } = 0;
        public int Publish { get; set; } = 0;
    }

    public class QueueConfiguration
    {
        public string Name { get; set; } = "";
        public List<String> Events { get; set; } = new List<String>();
    }
}
