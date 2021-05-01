namespace Fitmeplan.ServiceBus.EventAggregator
{
    public interface IEventAggregator
    {
        TEventType GetEvent<TEventType>() where TEventType : BaseEvent;
        void Clear();
    }
}