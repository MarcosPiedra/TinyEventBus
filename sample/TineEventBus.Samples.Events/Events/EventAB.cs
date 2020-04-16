using TinyEventBus.Events;

namespace TineEventBus.Samples.Events
{
    public class EventAB : EventBase
    {
        public string ValueAB { get; set; }
        public EventAB(string text)
        {
            ValueAB = text;
        }
    }
}
