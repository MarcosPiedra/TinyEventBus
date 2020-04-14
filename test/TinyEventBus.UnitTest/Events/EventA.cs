using System;
using System.Collections.Generic;
using System.Text;
using TinyEventBus.Events;

namespace TinyEventBus.UnitTest.Events
{
    public class EventA : EventBase
    {
        public string FieldAString { get; set; }
        public int FieldAInt { get; set; }
        public string Text { get; set; }
        public EventA(string text) => Text = text;
    }
}
