using System;
using System.Collections.Generic;
using System.Text;

namespace TinyEventBus
{
    public class EventHandlerType
    {
        public EventHandlerType(Type @event)
        {
            this.Type = @event;
        }

        public Type Type { get; private set; }
        public string Name => Type.FullName;
        public string RegisterName => $"handler-{this.Name}";

        public override int GetHashCode() => Type.GetHashCode() ^ Name.GetHashCode();
        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType())
                return false;

            var other = (EventHandlerType)obj;
            return this.GetHashCode() == other.GetHashCode();
        }
    }
}
