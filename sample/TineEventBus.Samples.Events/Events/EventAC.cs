using TinyEventBus.Events;

namespace TineEventBus.Samples.Events
{
    public class EventAC : EventBase
    {
        public string ValueAC { get; set; }
        public EventAC(string text)
        {
            ValueAC = text;
        }
    }
}
