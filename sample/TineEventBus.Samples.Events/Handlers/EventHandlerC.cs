using Microsoft.Extensions.Logging;
using TinyEventBus.Abstractions;
using TinyEventBus.Events;
using System;
using System.Threading.Tasks;

namespace TineEventBus.Samples.Events
{
    public class EventHandlerC : IEventHandler<EventC>,
                                 IEventHandler<EventBC>,
                                 IEventHandler<EventAC>,
                                 IEventHandler<EventABC>
    {
        private readonly IConsoleLogger log;

        public EventHandlerC(IConsoleLogger log)
        {
            this.log = log;
        }

        public async Task Handle(EventC @event)
        {
            log.Write($"Handle message, Type: {this.GetType().FullName} - Message: {@event.ValueC}");
            await Task.FromResult(0);
        }

        public async Task Handle(EventBC @event)
        {
            log.Write($"Handle message, Type: {this.GetType().FullName} - Message: {@event.ValueBC}");
            await Task.FromResult(0);
        }

        public async Task Handle(EventAC @event)
        {
            log.Write($"Handle message, Type: {this.GetType().FullName} - Message: {@event.ValueAC}");
            await Task.FromResult(0);
        }

        public async Task Handle(EventABC @event)
        {
            log.Write($"Handle message, Type: {this.GetType().FullName} - Message: {@event.ValueABC}");
            await Task.FromResult(0);
        }
    }
}



