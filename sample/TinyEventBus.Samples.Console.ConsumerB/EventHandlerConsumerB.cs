using Microsoft.Extensions.Logging;
using TinyEventBus.Abstractions;
using System;
using System.Threading.Tasks;
using TineEventBus.Samples.Events;

namespace TinyEventBus.Samples.Console.ConsumerB
{
    public class EventHandlerConsumerB : IEventHandler<EventAB>
    {
        private readonly IConsoleLogger log;

        public EventHandlerConsumerB(IConsoleLogger log)
        {
            this.log = log;
        }

        public async Task Handle(EventAB @event)
        {
            log.Write($"Handle message, Type: {this.GetType().FullName} - Message: {@event.ValueAB}");
            await Task.FromResult(0);
        }

    }
}



