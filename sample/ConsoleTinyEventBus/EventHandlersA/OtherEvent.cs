using TinyEventBus.Events;

namespace ConsoleTinyEventBus.EventHandlersA
{
    public class OtherEvent : EventBase
    {
        public string Text { get; internal set; }
        public OtherEvent(string text) => Text = text;
    }
}
