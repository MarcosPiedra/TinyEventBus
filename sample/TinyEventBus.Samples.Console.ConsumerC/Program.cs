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
using System.Text;
using TinyEventBus.DependencyInjection.Builder;
using System.Runtime.CompilerServices;
using TineEventBus.Samples.Events;
using SystemConsole = System.Console;
using System.Collections.Generic;

namespace TinyEventBus.Samples.Console.ConsumerC
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
                c.ConfigSection(tmpConfig);

                var assembly = typeof(EventA).Assembly;

                c.Register(assembly)
                    .AsConsumer()
                        .InQueue("QueueC")
                            .AddEvent("EventC")
                            .AddEvent("EventBC")
                            .AddEvent("EventAC")
                            .AddEvent("EventABC")
                            .ExcludeEventHandler("EventHandlerA")
                            .ExcludeEventHandler("EventHandlerB");
            });

            builder.RegisterInstance(loggerFactory.CreateLogger<RabbitMQConnection>());
            builder.RegisterInstance(loggerFactory.CreateLogger<WorkQueue>());
            builder.RegisterInstance(loggerFactory.CreateLogger<PubSub>());
            builder.RegisterInstance(loggerFactory.CreateLogger<EventBusRabbitMQ>());
            builder.RegisterInstance(loggerFactory.CreateLogger<RabbitMQConnection>());
            builder.RegisterType<ConsoleLogger>().As<IConsoleLogger>();
            var container = builder.Build();

            var bus = container.Resolve<IEventBus>();

            SystemConsole.WriteLine("Listening events in QueueC (press button for exit)");
            SystemConsole.ReadKey(true);
            SystemConsole.WriteLine("Exit!!");
        }
    }
}
