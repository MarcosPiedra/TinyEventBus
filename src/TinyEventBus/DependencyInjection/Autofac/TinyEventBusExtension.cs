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
            builder.RegisterType<Consumer>().As<IConsumer>();
            builder.RegisterType<RabbitMQConnection>().As<IRabbitMQConnection>();
            builder.RegisterType<ConsumerFactory>().As(typeof(IFactory<string, IRabbitMQConnection, IConsumer>));
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

            builder.Register<Func<string, IRabbitMQConnection, IConsumer>>(c =>
            {
                var context = c.Resolve<IComponentContext>();
                return (queueName, connection) =>
                {
                    var @params = new List<NamedParameter>
                    {
                        new NamedParameter("queueName", queueName),
                        new NamedParameter("connection", connection)
                    };
                    return context.Resolve<IConsumer>(@params);
                };
            });

            return builder;
        }
    }
}
