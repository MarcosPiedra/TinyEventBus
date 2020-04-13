using Autofac;
using Autofac.Core;
using Microsoft.Extensions.Configuration;
using RabbitMQ.Client;
using TinyEventBus.Abstractions;
using TinyEventBus.Events;
using TinyEventBus.RabbitMQ;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using TinyEventBus.Factory;
using ConnectionManager = TinyEventBus.RabbitMQ.ConnectionManager;
using TinyEventBus.Configuration;
using TinyEventBus.RabbitMQ.Connections;

namespace TinyEventBus.DependencyInjection.Autofac
{
    public static class TinyEventBusExtension
    {
        public static ContainerBuilder AddTinyEventBus(this ContainerBuilder builder, Action<ITinyEventBusBuilder> configure)
        {
            var configuration = new TinyEventBusBuilder();

            configure(configuration);

            configuration.RegisterHandlersManually();

            var suscriptionsManager = configuration.SubscriptionsManager;
            var factory = configuration.GetConnectionFactory();

            builder.RegisterType<RabbitMQConnection>()
                   .SingleInstance();

            builder.RegisterType<EventBusRabbitMQ>()
                   .As<IEventBus>()
                   .OnActivated(a => a.Instance.Init())
                   .SingleInstance();

            builder.RegisterInstance(configuration.Configure);
            builder.RegisterInstance(suscriptionsManager).As<ISubscriptionsManager>();
            builder.RegisterInstance(factory).As<IConnectionFactory>();
            builder.RegisterType<RabbitMQConnection>().As<IRabbitMQConnection>();
            builder.RegisterType<ConnectionManager>().As(typeof(IFactory<IConnectionStrategy>));
            builder.RegisterType<HandlerFactory>().As(typeof(IFactory<EventType, EventHandlerType, object>));

            foreach (var h in suscriptionsManager.GetEventHandlersByEvent())
            {
                builder.RegisterType(h.Item2.Type).Keyed(h.Item2.RegisterName, h.Item1.GenericEvent);
            }

            builder.Register<Func<EventType, EventHandlerType, object>>(c =>
            {
                var context = c.Resolve<IComponentContext>();
                return (eventType, eventHandlerType) => context.ResolveKeyed(eventHandlerType.RegisterName, eventType.GenericEvent);
            });

            builder.RegisterType<PubSub>().Keyed<IConnectionStrategy>(ConnectionStrategy.PubSub);
            builder.RegisterType<WorkQueue>().Keyed<IConnectionStrategy>(ConnectionStrategy.WorkQueue);

            builder.Register<Func<IConnectionStrategy>>(c =>
             {
                 var context = c.Resolve<IComponentContext>();
                 return () =>
                 {
                     var config = context.Resolve<TinyEventBusConfiguration>();
                     return context.ResolveKeyed<IConnectionStrategy>(config.ConnectionStrategy);
                 };
             });

            return builder;
        }
    }
}
