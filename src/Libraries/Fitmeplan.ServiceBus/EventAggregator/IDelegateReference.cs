using System;

namespace Fitmeplan.ServiceBus.EventAggregator
{
    public interface IDelegateReference
    {
        Delegate Target
        {
            get;
        }
    }
}