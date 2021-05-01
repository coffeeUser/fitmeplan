using System;

namespace Fitmeplan.ServiceBus.EventAggregator
{
    public class RpcEventSubscription<TPayload, TResponse> : IEventSubscription
    {
        private readonly IDelegateReference _actionReference;
        private readonly IDelegateReference _filterReference;

        public RpcEventSubscription(IDelegateReference actionReference, IDelegateReference filterReference)
        {
            if (actionReference == null)
            {
                throw new ArgumentNullException("actionReference");
            }

            if (filterReference == null)
            {
                throw new ArgumentNullException("filterReference");
            }

            if (!(actionReference.Target is Func<TPayload, TResponse>))
            {
                throw new ArgumentException("Invalid delegate rerefence type.", "actionReference");
            }

            if (!(filterReference.Target is Predicate<TPayload>))
            {
                throw new ArgumentException("Invalid delegate rerefence type.", "filterReference");
            }

            _actionReference = actionReference;
            _filterReference = filterReference;
        }

        public Func<TPayload, TResponse> Action
        {
            get
            {
                return (Func<TPayload, TResponse>) _actionReference.Target;
            }
        }

        public Predicate<TPayload> Filter
        {
            get
            {
                return (Predicate<TPayload>) _filterReference.Target;
            }
        }

        public SubscriptionToken SubscriptionToken
        {
            get;
            set;
        }

        public virtual MulticastDelegate GetExecutionStrategy()
        {
            Func<TPayload, TResponse> action = Action;
            Predicate<TPayload> filter = Filter;

            if (action != null && filter != null)
            {
                return (Func<TPayload, TResponse>)(argument =>
                                   {
                                       if (filter(argument))
                                       {
                                          return  InvokeAction(action, argument);
                                       }
                                       return default(TResponse);
                                   });
            }

            return null;
        }

        protected virtual TResponse InvokeAction(Func<TPayload, TResponse> action, TPayload argument)
        {
            return action(argument);
        }
    }
}