using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TinyEventBus.Abstractions;
using TinyEventBus.Events;

namespace TinyEventBus
{
    public class EventHandlerType
    {
        public EventHandlerType(Type handler)
        {
            if (!handler.GetInterfaces().Any(t => t.IsGenericType && t.GetGenericTypeDefinition() == typeof(IEventHandler<>)))
                throw new ArgumentException("Incorrect handler event");

            this.Type = handler;
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
