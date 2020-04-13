using Microsoft.Extensions.Logging;
using TinyEventBus.Abstractions;
using TinyEventBus.Events;
using System;
using System.Threading.Tasks;

namespace ConsoleTinyEventBus.EventHandlersA
{
    public class SampleEventHandler : IEventHandler<SampleEvent>
    {
        private readonly IConsoleLogger log;

        public SampleEventHandler(IConsoleLogger log)
        {
            this.log = log;
        }

        public async Task Handle(SampleEvent @event)
        {
            log.Write($"Handle message, Type: {this.GetType().FullName} - Message: {@event.Text}");
            await Task.FromResult(0);
        }
    }
}



