using TinyEventBus.Events;
using System.Threading.Tasks;

namespace TinyEventBus.Abstractions
{
    public interface IEventHandler<in TIntegrationEvent> : IEventHandler where TIntegrationEvent : EventBase
    {
        Task Handle(TIntegrationEvent @event);
    }

    public interface IEventHandler
    {
    }
}
