using Microsoft.Extensions.Logging;
using TinyEventBus.Abstractions;
using TinyEventBus.Events;
using System;
using System.Threading.Tasks;

namespace TineEventBus.Samples.Events
{
    public class EventHandlerB : IEventHandler<EventB>,
                                 IEventHandler<EventAB>,
                                 IEventHandler<EventBC>,
                                 IEventHandler<EventABC>
    {
        private readonly IConsoleLogger log;

        public EventHandlerB(IConsoleLogger log)
        {
            this.log = log;
        }

        public async Task Handle(EventB @event)
        {
            log.Write($"Handle message, Type: {this.GetType().FullName} - Message: {@event.ValueB}");
            await Task.FromResult(0);
        }

        public async Task Handle(EventAB @event)
        {
            log.Write($"Handle message, Type: {this.GetType().FullName} - Message: {@event.ValueAB}");
            await Task.FromResult(0);
        }

        public async Task Handle(EventBC @event)
        {
            log.Write($"Handle message, Type: {this.GetType().FullName} - Message: {@event.ValueBC}");
            await Task.FromResult(0);
        }

        public async Task Handle(EventABC @event)
        {
            log.Write($"Handle message, Type: {this.GetType().FullName} - Message: {@event.ValueABC}");
            await Task.FromResult(0);
        }
    }
}



