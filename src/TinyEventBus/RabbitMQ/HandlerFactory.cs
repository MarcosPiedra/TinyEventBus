using System;
using System.Collections.Generic;
using System.Text;
using TinyEventBus.Factory;

namespace TinyEventBus.RabbitMQ
{
    public class HandlerFactory : IFactory<EventType, EventHandlerType, object>
    {
        private readonly Func<EventType, EventHandlerType, object> getHandler;

        public HandlerFactory(Func<EventType, EventHandlerType, object> getHandler)
        {
            this.getHandler = getHandler;
        }

        public object Get(EventType eventType, EventHandlerType eventHandlerType) => getHandler(eventType, eventHandlerType);
    }
}
