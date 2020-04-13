using Autofac;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using TinyEventBus.Abstractions;
using TinyEventBus.RabbitMQ;
using System;
using System.Linq;
using TinyEventBus.DependencyInjection.Autofac;
using Autofac.Core;
using TinyEventBus.Events;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Json;
using System.IO;
using System.Reflection;
using TinyEventBus;
using TinyEventBus.RabbitMQ.Connections;

namespace ConsoleTinyEventBus
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var loggerFactory = LoggerFactory.Create(builder =>
            {
                builder.AddFilter("Microsoft", LogLevel.Warning)
                       .AddFilter("System", LogLevel.Warning)
                       .AddFilter("LoggingConsoleApp.Program", LogLevel.Debug);
            });

            var tmpConfig = new ConfigurationBuilder()
                                .SetBasePath(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location))
                                .AddJsonFile("appsettings.json").Build() as IConfiguration;

            var builder = new ContainerBuilder();
            builder.AddTinyEventBus(c =>
            {
                c.WithConfigSection(tmpConfig);
                c.Subscribe<EventHandlersB.OtherEvent, EventHandlersB.OtherEventHandler>("Queue1");
                c.Subscribe<EventHandlersB.OtherEvent, EventHandlersB.OtherEventHandler>("Queue2");
                c.Subscribe<EventHandlersA.SampleEvent, EventHandlersA.SampleEventHandler>("Queue1");
            });
            builder.RegisterInstance(loggerFactory.CreateLogger<RabbitMQConnection>());
            builder.RegisterInstance(loggerFactory.CreateLogger<WorkQueue>());
            builder.RegisterInstance(loggerFactory.CreateLogger<PubSub>());
            builder.RegisterInstance(loggerFactory.CreateLogger<EventBusRabbitMQ>());
            builder.RegisterInstance(loggerFactory.CreateLogger<RabbitMQConnection>());
            builder.RegisterType<ConsoleLogger>().As<IConsoleLogger>();
            var container = builder.Build();

            var bus = container.Resolve<IEventBus>();
            var log = container.Resolve<IConsoleLogger>();

            Console.Read();

            log.Write("Publish EventHandlersA.OtherEvent");
            bus.Publish(new EventHandlersA.OtherEvent("EventHandlersA.OtherEvent"));
            log.Write("Publish EventHandlersB.OtherEvent");
            bus.Publish(new EventHandlersB.OtherEvent("EventHandlersB.OtherEvent"));
            log.Write("Publish EventHandlersA.SampleEvent");
            bus.Publish(new EventHandlersA.SampleEvent("EventHandlersA.SampleEvent"));

            Console.Read();
        }
    }
}
