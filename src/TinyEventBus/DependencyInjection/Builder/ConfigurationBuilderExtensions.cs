using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using TinyEventBus.Configuration;

namespace TinyEventBus.DependencyInjection.Builder
{
    public static class ConfigurationBuilderExtensions
    {
        public static IRegisterBuilder Register(this IConfigurationBuilder builder, Assembly assembly)
        {
            var configBuilder = builder as ConfigurationBuilder;

            var register = new RegisterBuilder
            {
                Assembly = assembly
            };

            configBuilder.RegisterBuilders.Add(register);

            return register;
        }

        public static IConfigurationBuilder ConfigSection(this IConfigurationBuilder builder, IConfiguration configuration)
        {
            var configBuilder = builder as ConfigurationBuilder;

            configBuilder.Configure = configuration.GetSection("TinyEventBus")
                                                   .Get<TinyEventBusConfiguration>();

            foreach (var consumer in configBuilder.Configure.Consumers)
            {
                var register = new RegisterBuilder();
                register.Role = MQRole.Consumer;
                register.QueueName = consumer.QueueName;
                register.EventIncluded.AddRange(consumer.Events);
                register.EventHandlerExcluded.AddRange(consumer.ExcludeEventHandler);
                register.CurrentDomain = true;
                configBuilder.RegisterBuilders.Add(register);
            }

            foreach (var producer in configBuilder.Configure.Producers)
            {
                var register = new RegisterBuilder();
                register.Role = MQRole.Producer;
                register.QueueName = producer.QueueName;
                register.EventIncluded.AddRange(producer.Events);
                register.CurrentDomain = true;
                configBuilder.RegisterBuilders.Add(register);
            }

            return builder;
        }

        public static IRegisterBuilderConsumer AddConsumer(this IConfigurationBuilder builder)
        {
            var configBuilder = builder as ConfigurationBuilder;

            var register = new RegisterBuilder
            {
                Role = MQRole.Consumer,
                CurrentDomain = true
            };
            configBuilder.RegisterBuilders.Add(register);

            return register;
        }

        public static IRegisterBuilderProducer AddProducer(this IConfigurationBuilder builder)
        {
            var configBuilder = builder as ConfigurationBuilder;

            var register = new RegisterBuilder
            {
                Role = MQRole.Producer,
                CurrentDomain = true
            };
            configBuilder.RegisterBuilders.Add(register);

            return register;
        }
    }
}
