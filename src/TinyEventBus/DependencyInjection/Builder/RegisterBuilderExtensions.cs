using System;
using System.Collections.Generic;
using System.Text;

namespace TinyEventBus.DependencyInjection.Builder
{
    public static class RegisterBuilderExtensions
    {
        public static IRegisterBuilderConsumer AsConsumer(this IRegisterBuilder registerBuilder)
        {
            var register = registerBuilder as RegisterBuilder;
            register.Role = MQRole.Consumer;

            return register;
        }
        public static IRegisterBuilderConsumerQueue InQueue(this IRegisterBuilderConsumer registerBuilder, string name)
        {
            var register = registerBuilder as RegisterBuilder;
            register.QueueName = name;

            return register;
        }
        public static IRegisterBuilderConsumerAllEvents AllEvents(this IRegisterBuilderConsumerQueue registerBuilder)
        {
            var register = registerBuilder as RegisterBuilder;
            register.AllEvents = true;

            return register;
        }
        public static IRegisterBuilderConsumerAllEvents ExcludeEvent(this IRegisterBuilderConsumerAllEvents registerBuilder, string name)
        {
            var register = registerBuilder as RegisterBuilder;
            register.EventsExcludedInAll.Add(name);

            return register as IRegisterBuilderConsumerAllEvents;
        }
        public static IRegisterBuilderConsumerEvents AddEvent(this IRegisterBuilderConsumerQueue registerBuilder, string name)
        {
            var register = registerBuilder as RegisterBuilder;
            register.EventIncluded.Add(name);

            return register;
        }
        public static IRegisterBuilderConsumerEvents ExcludeEventHandler(this IRegisterBuilderConsumerQueue registerBuilder, string name)
        {
            var register = registerBuilder as RegisterBuilder;
            register.EventHandlerExcluded.Add(name);

            return register;
        }
        public static IRegisterBuilderConsumerEvents AddEvent(this IRegisterBuilderConsumerEvents registerBuilder, string name)
        {
            var register = registerBuilder as RegisterBuilder;
            register.EventIncluded.Add(name);

            return register;
        }
        public static IRegisterBuilderConsumerEvents ExcludeEventHandler(this IRegisterBuilderConsumerEvents registerBuilder, string name)
        {
            var register = registerBuilder as RegisterBuilder;
            register.EventHandlerExcluded.Add(name);

            return register;
        }
        public static IRegisterBuilderProducer AsProducer(this IRegisterBuilder registerBuilder)
        {
            var register = registerBuilder as RegisterBuilder;
            register.Role = MQRole.Producer;

            return register;
        }
        public static IRegisterBuilderProducerEvents AddEvent(this IRegisterBuilderProducer registerBuilder, string name)
        {
            var register = registerBuilder as RegisterBuilder;
            register.EventIncluded.Add(name);

            return register;
        }
        public static IRegisterBuilderProducerEvents AddEvent(this IRegisterBuilderProducerEvents registerBuilder, string name)
        {
            var register = registerBuilder as RegisterBuilder;
            register.EventIncluded.Add(name);

            return register;
        }
        public static IRegisterBuilderProducerAllEvents AllEvents(this IRegisterBuilderProducerQueue registerBuilder)
        {
            var register = registerBuilder as RegisterBuilder;
            register.AllEvents = true;

            return register;
        }
        public static IRegisterBuilderProducerAllEvents ExcludeEvent(this IRegisterBuilderProducerAllEvents registerBuilder, string name)
        {
            var register = registerBuilder as RegisterBuilder;

            register.EventsExcludedInAll.Add(name);

            return register;
        }
    }
}
