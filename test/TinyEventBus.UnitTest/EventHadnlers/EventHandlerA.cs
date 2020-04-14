using Microsoft.Extensions.Logging;
using TinyEventBus.Abstractions;
using TinyEventBus.Events;
using System;
using System.Threading.Tasks;
using TinyEventBus.UnitTest.Events;

namespace TinyEventBus.UnitTest.EventHandler
{
    public class EventHandlerA : IEventHandler<EventA>
    {
        public EventHandlerA()
        {
        }

        public async Task Handle(EventA @event)
        {
            await Task.FromResult(0);
        }
    }
}



