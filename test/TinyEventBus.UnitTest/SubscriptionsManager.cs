using System;
using Xunit;
using TinyEventBus;
using TinyEventBus.UnitTest.EventHandler;
using TinyEventBus.UnitTest.Events;
using System.Linq;
using Moq;
using System.Collections.Generic;
using System.Threading;

namespace TinyEventBus.UnitTest
{
    public class SubscriptionsManager
    {
        [Theory]
        [InlineData(1)]
        [InlineData(2)]
        [InlineData(10)]
        public void Should_Be_A_Number_Queue_Correct(int num)
        {
            var manager = new InMemorySubscriptionsManager() as ISubscriptionsManager;
            var queues = new List<string>();

            for (int i = 0; i < num; i++)
            {
                queues.Add($"q{i}");
                SetAllEventHandlers(manager, i);
            }

            Assert.Equal(queues.AsEnumerable(), manager.GetConsumersQueues());
        }

        [Fact]
        public void Should_Remove_And_Throw_Of_Events()
        {
            DoActionAndCheckEventsAndQueue(m => SetAllEventHandlers(m, 1),
                                           m => m.RemoveConsumer(NewE<EventA>()));

            DoActionAndCheckEventsAndQueue(m => SetAllEventHandlers(m, 1),
                                           m => m.RemoveConsumer(NewE<EventB>()));

            DoActionAndCheckEventsAndQueue(m => SetAllEventHandlers(m, 1),
                                           m =>
                                           {
                                               m.RemoveConsumer(NewE<EventA>(), NewEH<EventHandlerA>());
                                               m.RemoveConsumer(NewE<EventB>(), NewEH<EventHandlerB>());
                                           });
        }

        [Fact]
        public void Should_Remove_And_Throw_Events_And_Queue()
        {
            DoActionAndCheckEventsAndQueue(m =>
                                           {
                                               SetAllEventHandlers(m, 1);
                                               SetEventHandlersA(m, 1);
                                               SetEventHandlersB(m, 2);
                                           },
                                           m =>
                                           {
                                               m.RemoveConsumer("q1", NewE<EventA>());
                                           });

            DoActionAndCheckQueue(m =>
                                  {
                                      SetAllEventHandlers(m, 1);
                                      SetEventHandlersA(m, 1);
                                      SetEventHandlersB(m, 2);
                                  },
                                  m =>
                                  {
                                      m.RemoveConsumer("q1");
                                  });
        }

        [Fact]
        public void Should_Be_Events_And_Handler_Registered()
        {
            var manager = new InMemorySubscriptionsManager() as ISubscriptionsManager;
            var managerComparer = new Mock<ISubscriptionsManager>();
            var eventsAinQ1 = 0;

            managerComparer.Setup(m => m.AddOrUpdateConsumer(It.IsIn("q1"), It.IsIn(NewE<EventA>()), It.IsAny<EventHandlerType>()))
                           .Callback<string, EventType, EventHandlerType>((a, b, c) => eventsAinQ1++);

            SetEventHandlersA(manager, 1);
            SetEventHandlersA(managerComparer.Object, 1);
            SetEventHandlersA(manager, 2);
            SetEventHandlersA(managerComparer.Object, 2);
            SetEventHandlersB(manager, 2);
            SetEventHandlersB(managerComparer.Object, 2);

            var x = manager.GetConsumersEvents("q1", "x");
            Assert.Empty(x);

            x = manager.GetConsumersEvents("q1", nameof(EventA));
            Assert.Equal(2, eventsAinQ1);

            var y = manager.GetConsumersEvents("q1");
            Assert.Single(y);
        }

        private void DoActionAndCheckQueue(Action<ISubscriptionsManager> toAdd,
                                           Action<ISubscriptionsManager> toRemove)
        {
            List<string> beforeDeleteQueue = null;

            void toDo(ISubscriptionsManager m)
            {
                toAdd.Invoke(m);

                beforeDeleteQueue = new List<string>(m.GetConsumersQueues());

                toRemove.Invoke(m);
            }

            var queuesRemoved = new List<string>();
            var eventTypesRemoved = new List<EventType>();
            var manager = InitAndWaitRemoved(toDo, out queuesRemoved, out eventTypesRemoved);

            var currentQueue = manager.GetConsumersQueues();

            beforeDeleteQueue.RemoveAll(q => queuesRemoved.Remove(q));

            Assert.Equal(currentQueue, beforeDeleteQueue.AsEnumerable());
        }

        private void DoActionAndCheckEventsAndQueue(Action<ISubscriptionsManager> toAdd,
                                                    Action<ISubscriptionsManager> toRemove)
        {
            List<EventType> beforeDeleteEvents = null;
            List<string> beforeDeleteQueue = null;

            void toDo(ISubscriptionsManager m)
            {
                toAdd.Invoke(m);

                beforeDeleteQueue = new List<string>(m.GetConsumersQueues());
                beforeDeleteEvents = new List<EventType>(m.GetConsumersEvents().GroupBy(e => e.Item1).Select(e => e.Key));

                toRemove.Invoke(m);
            }

            var queuesRemoved = new List<string>();
            var eventTypesRemoved = new List<EventType>();
            var manager = InitAndWaitRemoved(toDo, out queuesRemoved, out eventTypesRemoved);


            var currentQueue = manager.GetConsumersQueues();
            beforeDeleteQueue.RemoveAll(q => queuesRemoved.Remove(q));
            Assert.Equal(currentQueue, beforeDeleteQueue.AsEnumerable());

            var currentEvents = manager.GetConsumersEvents().GroupBy(e => e.Item1).Select(e => e.Key);
            beforeDeleteEvents.RemoveAll(e => eventTypesRemoved.Remove(e));
            Assert.Equal(currentEvents, beforeDeleteEvents.AsEnumerable());

        }

        private ISubscriptionsManager InitAndWaitRemoved(Action<ISubscriptionsManager> toDo,
                                                         out List<string> queuesRemoved,
                                                         out List<EventType> eventTypesRemoved)
        {
            var manager = new InMemorySubscriptionsManager() as ISubscriptionsManager;

            var eventRemovedEvent = new AutoResetEvent(false);
            var eventsRemovedList = new List<EventType>();
            void eventRemoved(string queue, EventType @event)
            {
                eventsRemovedList.Add(@event);
                eventRemovedEvent.Set();
            }

            var queueRemovedEvent = new AutoResetEvent(false);
            var queueRemovedList = new List<string>();
            void queueRemoved(string queue)
            {
                queueRemovedList.Add(queue);
                queueRemovedEvent.Set();
            }

            manager.SetOnEventRemoved(eventRemoved);
            manager.SetOnQueueRemoved(queueRemoved);
            toDo.Invoke(manager);

            var resetEvents = new List<AutoResetEvent>() { queueRemovedEvent, eventRemovedEvent }.ToArray();
            WaitHandle.WaitAll(resetEvents, 1000);

            eventTypesRemoved = eventsRemovedList;
            queuesRemoved = queueRemovedList;

            return manager;
        }

        private void SetAllEventHandlers(ISubscriptionsManager manager, int queueNumber)
        {
            manager.AddOrUpdateConsumer($"q{queueNumber}", NewE<EventA>(), new EventHandlerType(typeof(EventHandlerA)));
            manager.AddOrUpdateConsumer($"q{queueNumber}", NewE<EventA>(), new EventHandlerType(typeof(EventHandlerA1)));
            manager.AddOrUpdateConsumer($"q{queueNumber}", NewE<EventB>(), new EventHandlerType(typeof(EventHandlerB)));
            manager.AddOrUpdateConsumer($"q{queueNumber}", NewE<EventB>(), new EventHandlerType(typeof(EventHandlerB1)));
        }

        private void SetEventHandlersA(ISubscriptionsManager manager, int i)
        {
            manager.AddOrUpdateConsumer($"q{i}", NewE<EventA>(), new EventHandlerType(typeof(EventHandlerA)));
            manager.AddOrUpdateConsumer($"q{i}", NewE<EventA>(), new EventHandlerType(typeof(EventHandlerA1)));
        }

        private void SetEventHandlersB(ISubscriptionsManager manager, int i)
        {
            manager.AddOrUpdateConsumer($"q{i}", NewE<EventB>(), new EventHandlerType(typeof(EventHandlerB)));
            manager.AddOrUpdateConsumer($"q{i}", NewE<EventB>(), new EventHandlerType(typeof(EventHandlerB1)));
        }

        private EventType NewE<T>() => new EventType(typeof(T));
        private EventHandlerType NewEH<T>() => new EventHandlerType(typeof(T));
    }
}
