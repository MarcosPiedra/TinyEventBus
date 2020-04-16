using TinyEventBus.Events;

namespace TineEventBus.Samples.Events
{
    public class EventABC : EventBase
    {
        public string ValueABC { get; set; }
        public EventABC(string text)
        {
            ValueABC = text;
        }
    }
}
