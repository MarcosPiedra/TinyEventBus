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

            var sb = new StringBuilder();
            sb.AppendLine("Option 1: Send a message from event EventHandlersA.OtherEvent");
            sb.AppendLine("Option 2: Send a message from event EventHandlersB.OtherEvent");
            sb.AppendLine("Option 3: Send a message from event EventHandlersA.SampleEvent");
            sb.AppendLine("Esc: Exit");

            Console.WriteLine(sb.ToString());

            ConsoleKeyInfo keyPressed;
            var rdm = new Random();

            do
            {
                keyPressed = Console.ReadKey(true);
                var randomText = string.Join("", "".PadLeft(7, 'A').Select(c => (char)((int)c + (int)rdm.Next(0, 25))).ToArray());

                if (keyPressed.Key == ConsoleKey.D1)
                {
                    Console.WriteLine($"Sending message {randomText} to OtherEvent");
                    bus.Publish(new EventHandlersA.OtherEvent(randomText));
                }
                else if (keyPressed.Key == ConsoleKey.D2)
                {
                    Console.WriteLine($"Sending message {randomText} to OtherEvent");
                    bus.Publish(new EventHandlersB.OtherEvent(randomText));
                }
                else if (keyPressed.Key == ConsoleKey.D3)
                {
                    Console.WriteLine($"Sending message {randomText} to SampleEvent");
                    bus.Publish(new EventHandlersA.SampleEvent(randomText));
                }
                else if (keyPressed.Key == ConsoleKey.Escape)
                {
                    Console.WriteLine($"Exit!");
                }
            } while (keyPressed.Key != ConsoleKey.Escape);
        }
    }
}
