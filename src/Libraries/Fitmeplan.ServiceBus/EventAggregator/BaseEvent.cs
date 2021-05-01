using System;
using System.Collections.Generic;
using System.Linq;

namespace Fitmeplan.ServiceBus.EventAggregator
{
    public abstract class BaseEvent
    {
        private readonly List<IEventSubscription> _subscriptions = new List<IEventSubscription>();

        /// <summary>
        /// Gets the subscriptions.
        /// </summary>
        /// <value>
        /// The subscriptions.
        /// </value>
        protected ICollection<IEventSubscription> Subscriptions
        {
            get { return _subscriptions; }
        }

        /// <summary>
        /// Subscribes to the specified event subscription.
        /// </summary>
        /// <param name="eventSubscription">The event subscription.</param>
        /// <returns></returns>
        protected virtual SubscriptionToken Subscribe(IEventSubscription eventSubscription)
        {
            eventSubscription.SubscriptionToken = new SubscriptionToken();

            lock (_subscriptions)
            {
                _subscriptions.Add(eventSubscription);
            }

            return eventSubscription.SubscriptionToken;
        }

        /// <summary>
        /// Publishes the event.
        /// </summary>
        /// <param name="arguments">The arguments.</param>
        protected virtual void Publish(params object[] arguments)
        {
            List<MulticastDelegate> executionStrategies = PruneAndReturnStrategies();

            foreach (var executionStrategy in executionStrategies)
            {
                ((Action<object[]>)executionStrategy)(arguments);
            }
        }

        /// <summary>
        /// Unsubscribes from event by token.
        /// </summary>
        /// <param name="token">The token.</param>
        public virtual void Unsubscribe(SubscriptionToken token)
        {
            lock (_subscriptions)
            {
                IEventSubscription subscription = _subscriptions.FirstOrDefault(evt => evt.SubscriptionToken == token);

                if (subscription != null)
                {
                    _subscriptions.Remove(subscription);
                }
            }
        }

        /// <summary>
        /// Determines whether [contains] [the specified token].
        /// </summary>
        /// <param name="token">The token.</param>
        /// <returns></returns>
        public virtual bool Contains(SubscriptionToken token)
        {
            lock (_subscriptions)
            {
                IEventSubscription subscription = _subscriptions.FirstOrDefault(evt => evt.SubscriptionToken == token);

                return (subscription != null);
            }
        }

        protected List<MulticastDelegate> PruneAndReturnStrategies()
        {
            List<MulticastDelegate> returnList = new List<MulticastDelegate>();

            lock (_subscriptions)
            {
                for (int i = _subscriptions.Count - 1; i >= 0; i--)
                {
                    MulticastDelegate listItem = _subscriptions[i].GetExecutionStrategy();

                    if (listItem == null)
                    {
                        _subscriptions.RemoveAt(i);
                    }
                    else
                    {
                        returnList.Add(listItem);
                    }
                }
            }

            return returnList;
        }
    }
}