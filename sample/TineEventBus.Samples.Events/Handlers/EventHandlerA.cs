using Microsoft.Extensions.Logging;
using TinyEventBus.Abstractions;
using TinyEventBus.Events;
using System;
using System.Threading.Tasks;

namespace TineEventBus.Samples.Events
{
    public class EventHandlerA : IEventHandler<EventA>,
                                 IEventHandler<EventAB>,
                                 IEventHandler<EventAC>,
                                 IEventHandler<EventABC>
    {
        private readonly IConsoleLogger log;

        public EventHandlerA(IConsoleLogger log)
        {
            this.log = log;
        }

        public async Task Handle(EventA @event)
        {
            log.Write($"Handle message, Type: {this.GetType().FullName} - Message: {@event.ValueA}");
            await Task.FromResult(0);
        }

        public async Task Handle(EventAB @event)
        {
            log.Write($"Handle message, Type: {this.GetType().FullName} - Message: {@event.ValueAB}");
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



