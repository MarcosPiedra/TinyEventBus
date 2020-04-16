using TinyEventBus.Events;

namespace TinyEventBus.Samples.Console.ConsumerB
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
