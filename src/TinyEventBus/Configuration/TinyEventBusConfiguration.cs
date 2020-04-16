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
        public List<ConsumerConfiguration> Consumers { get; set; } = new List<ConsumerConfiguration>();
        public List<ProducerConfiguration> Producers { get; set; } = new List<ProducerConfiguration>();
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

    public class ConsumerConfiguration
    {
        public string QueueName { get; set; } = "";
        public List<string> Events { get; set; } = new List<string>();
        public List<string> ExcludeEventHandler { get; set; } = new List<string>();
    }
    public class ProducerConfiguration
    {
        public string QueueName { get; set; } = "";
        public List<string> Events { get; set; } = new List<string>();
    }
}
