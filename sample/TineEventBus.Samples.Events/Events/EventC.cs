using System;
using System.Collections.Generic;
using System.Text;
using TinyEventBus.Events;

namespace TineEventBus.Samples.Events
{
    public class EventC : EventBase
    {
        public string ValueC { get; set; }
        public EventC(string text)
        {
            ValueC = text;
        }
    }
}
