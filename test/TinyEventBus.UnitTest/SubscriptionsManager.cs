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

            Assert.Equal(queues.AsEnumerable(), manager.GetQueues());
        }

        [Fact]
        public void Should_Remove_And_Throw_Of_Events()
        {
            DoActionAndCheckEventsAndQueue(m => SetAllEventHandlers(m, 1),
                                           m => m.RemoveSubscriptions(NewE<EventA>()));

            DoActionAndCheckEventsAndQueue(m => SetAllEventHandlers(m, 1),
                                           m => m.RemoveSubscriptions(NewE<EventB>()));

            DoActionAndCheckEventsAndQueue(m => SetAllEventHandlers(m, 1),
                                           m =>
                                           {
                                               m.RemoveSubscriptions(NewE<EventA>(), NewEH<EventHandlerA>());
                                               m.RemoveSubscriptions(NewE<EventB>(), NewEH<EventHandlerB>());
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
                                               m.RemoveSubscriptions("q1", NewE<EventA>());
                                           });

            DoActionAndCheckQueue(m =>
                                  {
                                      SetAllEventHandlers(m, 1);
                                      SetEventHandlersA(m, 1);
                                      SetEventHandlersB(m, 2);
                                  },
                                  m =>
                                  {
                                      m.RemoveSubscriptions("q1");
                                  });
        }

        [Fact]
        public void Should_Be_Events_And_Handler_Registered()
        {
            var manager = new InMemorySubscriptionsManager() as ISubscriptionsManager;
            var managerComparer = new Mock<ISubscriptionsManager>();
            var eventsAinQ1 = 0;

            managerComparer.Setup(m => m.AddSubscription(It.IsIn("q1"), It.IsIn(NewE<EventA>()), It.IsAny<EventHandlerType>()))
                           .Callback<string, EventType, EventHandlerType>((a, b, c) => eventsAinQ1++);

            SetEventHandlersA(manager, 1);
            SetEventHandlersA(managerComparer.Object, 1);
            SetEventHandlersA(manager, 2);
            SetEventHandlersA(managerComparer.Object, 2);
            SetEventHandlersB(manager, 2);
            SetEventHandlersB(managerComparer.Object, 2);

            var x = manager.GetEventHandlersByEvent("q1", "x");
            Assert.Empty(x);

            x = manager.GetEventHandlersByEvent("q1", nameof(EventA));
            Assert.Equal(2, eventsAinQ1);

            var y = manager.GetEventsNameGrouped("q1");
            Assert.Single(y);
        }

        private void DoActionAndCheckQueue(Action<ISubscriptionsManager> toAdd,
                                           Action<ISubscriptionsManager> toRemove)
        {
            List<string> beforeDeleteQueue = null;

            void toDo(ISubscriptionsManager m)
            {
                toAdd.Invoke(m);

                beforeDeleteQueue = new List<string>(m.GetQueues());

                toRemove.Invoke(m);
            }

            var queuesRemoved = new List<string>();
            var eventTypesRemoved = new List<EventType>();
            var manager = InitAndWaitRemoved(toDo, out queuesRemoved, out eventTypesRemoved);

            var currentQueue = manager.GetQueues();

            beforeDeleteQueue.RemoveAll(q => queuesRemoved.Remove(q));

            Assert.Equal(currentQueue, beforeDeleteQueue.AsEnumerable());
        }

        private void DoActionAndCheckEventsAndQueue(Action<ISubscriptionsManager> toAdd,
                                                    Action<ISubscriptionsManager> toRemove)
        {
            List<EventType> beforeDeleteA = null;
            List<EventType> beforeDeleteB = null;
            List<string> beforeDeleteQueue = null;

            void toDo(ISubscriptionsManager m)
            {
                toAdd.Invoke(m);

                beforeDeleteQueue = new List<string>(m.GetQueues());
                beforeDeleteA = new List<EventType>(m.GetEvents(nameof(EventA)));
                beforeDeleteB = new List<EventType>(m.GetEvents(nameof(EventB)));

                toRemove.Invoke(m);
            }

            var queuesRemoved = new List<string>();
            var eventTypesRemoved = new List<EventType>();
            var manager = InitAndWaitRemoved(toDo, out queuesRemoved, out eventTypesRemoved);

            var currentA = manager.GetEvents(nameof(EventA));
            var currentB = manager.GetEvents(nameof(EventB));
            var currentQueue = manager.GetQueues();

            beforeDeleteA.RemoveAll(e => eventTypesRemoved.Contains(e));
            beforeDeleteB.RemoveAll(e => eventTypesRemoved.Contains(e));
            beforeDeleteQueue.RemoveAll(q => queuesRemoved.Remove(q));

            Assert.Equal(currentA, beforeDeleteA.AsEnumerable());
            Assert.Equal(currentB, beforeDeleteB.AsEnumerable());
            Assert.Equal(currentQueue, beforeDeleteQueue.AsEnumerable());
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
            manager.AddSubscription($"q{queueNumber}", NewE<EventA>(), new EventHandlerType(typeof(EventHandlerA)));
            manager.AddSubscription($"q{queueNumber}", NewE<EventA>(), new EventHandlerType(typeof(EventHandlerA1)));
            manager.AddSubscription($"q{queueNumber}", NewE<EventB>(), new EventHandlerType(typeof(EventHandlerB)));
            manager.AddSubscription($"q{queueNumber}", NewE<EventB>(), new EventHandlerType(typeof(EventHandlerB1)));
        }

        private void SetEventHandlersA(ISubscriptionsManager manager, int i)
        {
            manager.AddSubscription($"q{i}", NewE<EventA>(), new EventHandlerType(typeof(EventHandlerA)));
            manager.AddSubscription($"q{i}", NewE<EventA>(), new EventHandlerType(typeof(EventHandlerA1)));
        }

        private void SetEventHandlersB(ISubscriptionsManager manager, int i)
        {
            manager.AddSubscription($"q{i}", NewE<EventB>(), new EventHandlerType(typeof(EventHandlerB)));
            manager.AddSubscription($"q{i}", NewE<EventB>(), new EventHandlerType(typeof(EventHandlerB1)));
        }

        private EventType NewE<T>() => new EventType(typeof(T));
        private EventHandlerType NewEH<T>() => new EventHandlerType(typeof(T));
    }
}
