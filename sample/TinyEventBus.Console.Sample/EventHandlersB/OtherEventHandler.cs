using Microsoft.Extensions.Logging;
using TinyEventBus.Abstractions;
using TinyEventBus.Events;
using System;
using System.Threading.Tasks;

namespace ConsoleTinyEventBus.EventHandlersB
{
    public class OtherEventHandler : IEventHandler<OtherEvent>
    {
        private readonly IConsoleLogger log;

        public OtherEventHandler(IConsoleLogger log)
        {
            this.log = log;
        }

        public async Task Handle(OtherEvent @event)
        {
            log.Write($"Handle message, Type: {this.GetType().FullName} - Message: {@event.Text}");
            await Task.FromResult(0);
        }
    }
}



