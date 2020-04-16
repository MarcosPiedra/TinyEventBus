using TinyEventBus.Events;

namespace TineEventBus.Samples.Events
{
    public class EventA : EventBase
    {
        public string ValueA { get; set; }
        public EventA(string text)
        {
            ValueA = text;
        }
    }
}
