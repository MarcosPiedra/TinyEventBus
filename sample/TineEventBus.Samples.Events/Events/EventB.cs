using System;
using System.Collections.Generic;
using System.Text;
using TinyEventBus.Events;

namespace TineEventBus.Samples.Events
{
    public class EventB : EventBase
    {
        public string ValueB { get; set; }
        public EventB(string text)
        {
            ValueB = text;
        }
    }
}
