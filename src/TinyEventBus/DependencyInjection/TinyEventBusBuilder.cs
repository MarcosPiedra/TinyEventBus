using Microsoft.Extensions.Configuration;
using RabbitMQ.Client;
using TinyEventBus.Abstractions;
using TinyEventBus.Events;
using TinyEventBus.RabbitMQ;
using System;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using Autofac;
using TinyEventBus.Configuration;

namespace TinyEventBus.DependencyInjection
{
    public class TinyEventBusBuilder : ITinyEventBusBuilder
    {
        public ISubscriptionsManager SubscriptionsManager { get; internal set; } = new InMemorySubscriptionsManager();
        public TinyEventBusConfiguration Configure { get; internal set; } = new TinyEventBusConfiguration();

        public TinyEventBusBuilder()
        {
        }

        public ITinyEventBusBuilder RegisterAllHandlersOf(string queue, Assembly assembly)
        {
            var hs = assembly.GetTypes()
                             .Where(t => t.IsClosedTypeOf(typeof(IEventHandler<>)))
                             .ToList();

            foreach (var handler in hs)
            {
                RegisterSuscription(queue, handler);
            }

            return this;
        }

        public ITinyEventBusBuilder Subscribe<T, TH>(string queue)
            where T : EventBase
            where TH : IEventHandler<T>
        {
            RegisterSuscription(queue, typeof(TH), typeof(T));
            return this;
        }

        public ITinyEventBusBuilder WithConfigSection(IConfiguration configuration)
        {
            Configure = configuration.GetSection("TinyEventBus")
                                     .Get<TinyEventBusConfiguration>();

            return this;
        }

        public IConnectionFactory GetConnectionFactory()
        {
            return new ConnectionFactory()
            {
                HostName = Configure.Hostname,
                UserName = Configure.Username,
                Password = Configure.Password,
                Port = Configure.Port,
                DispatchConsumersAsync = true,
            };
        }

        public void RegisterHandlersManually()
        {
            var eventsDefined = Configure.Queues.SelectMany(q => q.Events).FirstOrDefault();

            if (eventsDefined == null)
                return;

            var scanner = AppDomain.CurrentDomain
                                   .GetAssemblies()
                                   .ToList()
                                   .SelectMany(x => x.GetTypes())
                                   .Where(t => t.IsClosedTypeOf(typeof(IEventHandler<>)))
                                   .Select(t => new { h = t, e = t.GetInterfaces()[0].GenericTypeArguments[0] });

            foreach (var queue in Configure.Queues)
            {
                var queueName = queue.Name;
                foreach (var eventName in queue.Events)
                {
                    var @eventHandlers = scanner.Where(t => t.e.Name == eventName);
                    foreach (var he in @eventHandlers)
                    {
                        RegisterSuscription(queueName, he.h, he.e);
                    }
                }
            }
        }

        private void RegisterSuscription(string queue, Type handler, Type @event = null)
        {
            @event = @event ?? handler.GetInterfaces()[0].GenericTypeArguments[0];
            SubscriptionsManager.AddSubscription(queue, new EventType(@event), new EventHandlerType(handler));
        }
    }
}