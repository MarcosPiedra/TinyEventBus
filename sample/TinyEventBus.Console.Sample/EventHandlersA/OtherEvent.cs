using TinyEventBus.Events;

namespace ConsoleTinyEventBus.EventHandlersA
{
    public class OtherEvent : EventBase
    {
        public string Name { get; set; } = "Name - OtherEvent";
        public string Text { get; set; }
        public OtherEvent(string text) => Text = text;
    }
}
