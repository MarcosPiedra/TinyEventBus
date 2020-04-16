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
    public class ConfigurationBuilder : IConfigurationBuilder
    {
        public ISubscriptionsManager SubscriptionsManager { get; internal set; } = new InMemorySubscriptionsManager();
        public TinyEventBusConfiguration Configure { get; internal set; } = new TinyEventBusConfiguration();
        public List<RegisterBuilder> RegisterBuilders { get; } = new List<RegisterBuilder>();

        public ConfigurationBuilder()
        {
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

        public void DoRegister()
        {
            foreach (var register in RegisterBuilders)
            {
                var assemblies = new List<Assembly>();
                var eventsIncluded = new List<string>();
                var eventsExcluded = new List<string>();
                var eventHandlersExcluded = new List<string>();

                if (register.CurrentDomain)
                {
                    assemblies.AddRange(AppDomain.CurrentDomain.GetAssemblies());
                }
                else
                {
                    assemblies.Add(register.Assembly);
                }

                if (register.AllEvents)
                {
                    eventsExcluded.AddRange(register.EventsExcludedInAll);
                }
                else
                {
                    eventHandlersExcluded.AddRange(register.EventHandlerExcluded);
                    eventsIncluded.AddRange(register.EventIncluded);
                }

                foreach (var assembly in assemblies)
                {
                    if (register.Role == MQRole.Consumer)
                    {
                        RegisterConsumers(register.QueueName, eventsIncluded, eventsExcluded, eventHandlersExcluded, assembly);
                    }
                    else if (register.Role == MQRole.Producer)
                    {
                        RegisterProducers(eventsIncluded, eventsExcluded, assembly);
                    }
                }
            }
        }

        private void RegisterProducers(List<string> eventsIncluded, List<string> eventsExcluded, Assembly assembly)
        {
            var scanner = assembly.GetTypes()
                                  .Where(t => t.BaseType != null && t.BaseType.Equals(typeof(EventBase)))
                                  .Select(t => new EventType(t))
                                  .Where(e =>
                                  {
                                      if (eventsIncluded.Count > 0)
                                          return eventsIncluded.Contains(e.Name);

                                      if (eventsExcluded.Count > 0)
                                          return !eventsExcluded.Contains(e.Name);

                                      return true;
                                  })
                                  .AsEnumerable();

            foreach (var e in scanner)
            {
                SubscriptionsManager.AddOrUpdateProducer(e);
            }
        }

        public void RegisterConsumers(string queueName,
                                      List<string> includedEvents,
                                      List<string> excludeEvents,
                                      List<string> excludeEventHandlers,
                                      Assembly assembly)
        {
            var scanner = assembly.GetTypes()
                                  .Where(t => t.IsClosedTypeOf(typeof(IEventHandler<>)))
                                  .SelectMany(t => t.GetInterfaces().Where(i => i.GetGenericArguments().Count() > 0).Select(i => new { h = new EventHandlerType(t), e = new EventType(i.GenericTypeArguments[0]) }))
                                  .Where(t =>
                                  {
                                      var include = true;
                                      if (include && includedEvents.Count > 0)
                                          include = includedEvents.Contains(t.e.Name);

                                      if (include && excludeEvents.Count > 0)
                                          include = !excludeEvents.Contains(t.e.Name);

                                      if (include && excludeEventHandlers.Count > 0)
                                          include = !excludeEventHandlers.Contains(t.h.Name);

                                      return include;
                                  })
                                  .AsEnumerable().ToList();

            foreach (var types in scanner)
            {
                SubscriptionsManager.AddOrUpdateConsumer(queueName, types.e, types.h);
            }
        }
    }
}