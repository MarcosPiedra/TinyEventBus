using TinyEventBus.Events;

namespace ConsoleTinyEventBus.EventHandlersA
{
    public class SampleEvent : EventBase
    {
        public string Text { get; internal set; }
        public SampleEvent(string text) => Text = text;
    }
}
