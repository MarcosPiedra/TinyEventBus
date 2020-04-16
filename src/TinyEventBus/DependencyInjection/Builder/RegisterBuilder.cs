using System.Collections.Generic;
using System.Reflection;
using TinyEventBus.DependencyInjection.Builder;

namespace TinyEventBus.DependencyInjection
{
    public enum MQRole
    {
        Producer,
        Consumer,
    }

    public class RegisterBuilder : IRegisterBuilder,
                                   IRegisterBuilderConsumer, IRegisterBuilderConsumerQueue, IRegisterBuilderConsumerEvents, IRegisterBuilderConsumerAllEvents,
                                   IRegisterBuilderProducer, IRegisterBuilderProducerQueue, IRegisterBuilderProducerEvents, IRegisterBuilderProducerAllEvents
    {
        public Assembly Assembly { get; set; } = null;
        public bool CurrentDomain { get; set; } = false;
        public bool AllEvents { get; set; } = false;
        public List<string> EventsExcludedInAll { get; } = new List<string>();
        public MQRole Role { get; set; }
        public string QueueName { get; set; } = "";
        public List<string> EventIncluded { get; } = new List<string>();
        public List<string> EventHandlerExcluded { get; } = new List<string>();
    }
}