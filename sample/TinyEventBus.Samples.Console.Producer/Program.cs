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

namespace TinyEventBus.Samples.Console.Producer
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
                    .AsProducer()
                        .AddEvent("EventA")
                        .AddEvent("EventAB")
                        .AddEvent("EventAC")
                        .AddEvent("EventABC");

                c.Register(assembly)
                    .AsProducer()
                        .AddEvent("EventB")
                        .AddEvent("EventAB")
                        .AddEvent("EventBC")
                        .AddEvent("EventABC");

                // Configured in appsettings
                //c.Register(assembly)
                //    .AsProducer()
                //       .AddEvent("EventC")
                //       .AddEvent("EventAC")
                //       .AddEvent("EventBC")
                //       .AddEvent("EventABC");

            });

            builder.RegisterInstance(loggerFactory.CreateLogger<RabbitMQConnection>());
            builder.RegisterInstance(loggerFactory.CreateLogger<WorkQueue>());
            builder.RegisterInstance(loggerFactory.CreateLogger<PubSub>());
            builder.RegisterInstance(loggerFactory.CreateLogger<EventBusRabbitMQ>());
            builder.RegisterInstance(loggerFactory.CreateLogger<RabbitMQConnection>());
            builder.RegisterType<ConsoleLogger>().As<IConsoleLogger>();
            var container = builder.Build();

            var bus = container.Resolve<IEventBus>();

            var eventsToSend = new List<Type>();
            eventsToSend.Add(typeof(EventA));
            eventsToSend.Add(typeof(EventAC));
            eventsToSend.Add(typeof(EventAB));
            eventsToSend.Add(typeof(EventB));
            eventsToSend.Add(typeof(EventBC));
            eventsToSend.Add(typeof(EventC));
            eventsToSend.Add(typeof(EventABC));

            var sb = new StringBuilder();
            var indx = 0;
            foreach (var e in eventsToSend)
            {
                sb.AppendLine($"Option {indx++}: Send {e.Name}");
            }
            sb.AppendLine("Esc: Exit");

            SystemConsole.WriteLine(sb.ToString());

            ConsoleKeyInfo keyPressed;          
            do
            {
                keyPressed = SystemConsole.ReadKey(true);

                var option = (int)char.GetNumericValue(keyPressed.KeyChar);
                if (option >= 0 && option < eventsToSend.Count)
                {
                    var rdm = new Random();
                    var randomText = string.Join("", "".PadLeft(7, 'A').Select(c => (char)(c + rdm.Next(0, 25))).ToArray());

                    var @event = eventsToSend[option];
                    var instanceToSend = Activator.CreateInstance(@event, new object[] { randomText });
                    var method = bus.GetType().GetMethod("Publish");
                    var generic = method.MakeGenericMethod(@event);
                    generic.Invoke(bus, new object[] { instanceToSend });

                    SystemConsole.WriteLine($"Selected {option}: {@event.Name} sent with text {randomText}");
                }

            } while (keyPressed.Key != ConsoleKey.Escape);

            SystemConsole.WriteLine("Exit!!");
        }
    }
}
