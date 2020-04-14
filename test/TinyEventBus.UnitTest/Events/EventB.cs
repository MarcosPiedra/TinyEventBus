using System;
using System.Collections.Generic;
using System.Text;
using TinyEventBus.Events;

namespace TinyEventBus.UnitTest.Events
{
    public class EventB : EventBase
    {
        public string FieldBString { get; set; }
        public int FielBdInt { get; set; }
        public string Text { get; set; }
        public EventB(string text) => Text = text;
    }
}
