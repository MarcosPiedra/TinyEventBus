using TinyEventBus.Events;

namespace TineEventBus.Samples.Events
{
    public class EventBC : EventBase
    {
        public string ValueBC { get; set; }
        public EventBC(string text)
        {
            ValueBC = text;
        }
    }
}
