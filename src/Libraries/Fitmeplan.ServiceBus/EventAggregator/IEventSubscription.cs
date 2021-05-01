using System;

namespace Fitmeplan.ServiceBus.EventAggregator
{
    public interface IEventSubscription
    {
        SubscriptionToken SubscriptionToken
        {
            get;
            set;
        }

        MulticastDelegate GetExecutionStrategy();
    }
}