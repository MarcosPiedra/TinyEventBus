using Microsoft.Extensions.Configuration;
using TinyEventBus.Abstractions;
using TinyEventBus.Events;
using System.Reflection;

namespace TinyEventBus.DependencyInjection
{
    public interface ITinyEventBusBuilder
    {
        ITinyEventBusBuilder WithConfigSection(IConfiguration configuration);
        ITinyEventBusBuilder RegisterAllHandlersOf(string queue, Assembly assembly);
        ITinyEventBusBuilder Subscribe<T, TH>(string queue) where T : EventBase where TH : IEventHandler<T>;
    }
}