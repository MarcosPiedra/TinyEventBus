using Microsoft.Extensions.Logging;
using TinyEventBus.Abstractions;
using TinyEventBus.Events;
using System;
using System.Threading.Tasks;
using TinyEventBus.UnitTest.Events;

namespace TinyEventBus.UnitTest.EventHandler
{
    public class EventHandlerB1 : IEventHandler<EventB>
    {
        public EventHandlerB1()
        {
        }

        public async Task Handle(EventB @event)
        {
            await Task.FromResult(0);
        }
    }
}



