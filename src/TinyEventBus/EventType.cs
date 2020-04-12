using System;
using System.Collections.Generic;
using System.Text;
using TinyEventBus.Abstractions;

namespace TinyEventBus
{
    public class EventType
    {
        public EventType(Type @event)
        {
            this.Type = @event;
        }

        public Type Type { get; private set; } = null;
        public string Name => this.Type.FullName;
        public Type GenericEvent => typeof(IEventHandler<>).MakeGenericType(Type);

        public override int GetHashCode() => Type.GetHashCode() ^ Name.GetHashCode();
        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType())
                return false;

            var other = (EventType)obj;
            return this.GetHashCode() == other.GetHashCode();
        }
    }
}
