using System;
using System.Collections.Generic;
using System.Text;
using TinyEventBus.Events;

namespace ConsoleTinyEventBus.EventHandlersB
{
    public class OtherEvent : EventBase
    {
        public string Text { get; set; }
        public OtherEvent(string text) => Text = text;
    }
}
