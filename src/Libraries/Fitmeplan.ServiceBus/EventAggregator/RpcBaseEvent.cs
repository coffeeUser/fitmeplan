using System;
using System.Linq;
using System.Threading.Tasks;

namespace Fitmeplan.ServiceBus.EventAggregator
{
    public class RpcBaseEvent<TPayload, TResponse> : BaseEvent
    {
        /// <summary>
        /// Subscribes the specified action.
        /// </summary>
        /// <param name="action">The action.</param>
        /// <returns></returns>
        public virtual SubscriptionToken Subscribe(Func<TPayload, TResponse> action)
        {
            return Subscribe(action, false);
        }

        public SubscriptionToken SubscribeAsync(Func<TPayload, Task<TResponse>> action)
        {
            Func<TPayload, TResponse> func = payload => action(payload).Result;
            return Subscribe(func, true);
        }

        /// <summary>
        /// Subscribes the specified action.
        /// </summary>
        /// <param name="action">The action.</param>
        /// <param name="keepSubscriberReferenceAlive">if set to <c>true</c> [keep subscriber reference alive].</param>
        /// <returns></returns>
        public virtual SubscriptionToken Subscribe(Func<TPayload, TResponse> action, bool keepSubscriberReferenceAlive)
        {
            return Subscribe(action, keepSubscriberReferenceAlive, delegate { return true; });
        }

        /// <summary>
        /// Subscribes the specified action.
        /// </summary>
        /// <param name="action">The action.</param>
        /// <param name="keepSubscriberReferenceAlive">if set to <c>true</c> [keep subscriber reference alive].</param>
        /// <param name="filter">The filter.</param>
        /// <returns></returns>
        public virtual SubscriptionToken Subscribe(Func<TPayload, TResponse> action, bool keepSubscriberReferenceAlive, Predicate<TPayload> filter)
        {
            IDelegateReference actionReference = new DelegateReference(action, keepSubscriberReferenceAlive);
            IDelegateReference filterReference = new DelegateReference(filter, keepSubscriberReferenceAlive);

            RpcEventSubscription<TPayload, TResponse> subscription = new RpcEventSubscription<TPayload, TResponse>(actionReference, filterReference);

            return base.Subscribe(subscription);
        }

        /// <summary>
        /// Publishes the event.
        /// </summary>
        /// <param name="payload">The payload.</param>
        /// <returns></returns>
        /// <exception cref="EventNotFoundException"></exception>
        public virtual TResponse Publish(TPayload payload)
        {
            Func<TPayload, TResponse> executionStrategy = (Func<TPayload, TResponse>)PruneAndReturnStrategies().FirstOrDefault();

            if (executionStrategy == null)
            {
                throw new EventNotFoundException($"Subscribers for message of type [{typeof(TPayload).Name}] not found");
            }

            return executionStrategy(payload);
        }

        /// <summary>
        /// Unsubscribes the specified subscriber.
        /// </summary>
        /// <param name="subscriber">The subscriber.</param>
        public virtual void Unsubscribe(Func<TPayload, TResponse> subscriber)
        {
            lock (Subscriptions)
            {
                IEventSubscription eventSubscription = Subscriptions.Cast<RpcEventSubscription<TPayload, TResponse>>().FirstOrDefault(evt => evt.Action == subscriber);

                if (eventSubscription != null)
                {
                    Subscriptions.Remove(eventSubscription);
                }
            }
        }

        /// <summary>
        /// Determines whether [contains] [the specified subscriber].
        /// </summary>
        /// <param name="subscriber">The subscriber.</param>
        /// <returns>
        ///   <c>true</c> if [contains] [the specified subscriber]; otherwise, <c>false</c>.
        /// </returns>
        public virtual bool Contains(Func<TPayload, TResponse> subscriber)
        {
            IEventSubscription eventSubscription;

            lock (Subscriptions)
            {
                eventSubscription = Subscriptions.Cast<RpcEventSubscription<TPayload, TResponse>>().FirstOrDefault(evt => evt.Action == subscriber);
            }

            return eventSubscription != null;
        }
    }
}