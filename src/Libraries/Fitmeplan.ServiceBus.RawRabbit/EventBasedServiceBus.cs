using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using Fitmeplan.Identity;
using Fitmeplan.ServiceBus.Core;
using Fitmeplan.ServiceBus.EventAggregator;

namespace Fitmeplan.ServiceBus.RawRabbit
{
    public class EventBasedServiceBus : IServiceBus
    {
        private readonly IContextAccessor _identityAccessor;

        /// <summary>Initializes a new instance of the <see cref="T:System.Object"></see> class.</summary>
        public EventBasedServiceBus(IContextAccessor identityAccessor)
        {
            _identityAccessor = identityAccessor;
        }

        public Task PublishAsync<TRequest>(TRequest request, ICorrelationContext context = null)
            where TRequest : class
        {
            SetContext();
            EventAggregatorFactory.EventAggregator.GetEvent<BaseEvent<TRequest>>().Publish(request);
            return Task.CompletedTask;
        }

        public Task<TResponse> RequestAsync<TRequest, TResponse>(TRequest request, ICorrelationContext context = null) 
            where TRequest : class 
            where TResponse : ResponseMessage, new()
        {
            SetContext();
            return Task.FromResult(EventAggregatorFactory.EventAggregator.GetEvent<RpcBaseEvent<TRequest, TResponse>>().Publish(request));
        }

        public Task SubscribeAsync<TRequest>(Func<TRequest, Task> handler) 
            where TRequest : class 
        {
            EventAggregatorFactory.EventAggregator.GetEvent<BaseEvent<TRequest>>().SubscribeAsync(handler);
            return Task.CompletedTask;
        }

        public Task SubscribeAsync<TRequest, TResponse>(Func<TRequest, Task<TResponse>> handler) 
            where TRequest : class 
            where TResponse : ResponseMessage, new()
        {
            EventAggregatorFactory.EventAggregator.GetEvent<RpcBaseEvent<TRequest, TResponse>>().SubscribeAsync(handler);
            return Task.CompletedTask;
        }

        public void Unsubscribe()
        {
            EventAggregatorFactory.EventAggregator.Clear();
        }

        public void Unsubscribe<TRequest>(Action<TRequest> handler)
        {
            EventAggregatorFactory.EventAggregator.GetEvent<BaseEvent<TRequest>>().Unsubscribe(handler);
        }

        public void Unsubscribe<TRequest, TResponse>(Func<TRequest, TResponse> handler)
        {
            EventAggregatorFactory.EventAggregator.GetEvent<RpcBaseEvent<TRequest, TResponse>>().Unsubscribe(handler);
        }

        private void SetContext()
        {
            var claimsPrincipal = _identityAccessor.CurrentPrincipal;
            Thread.CurrentPrincipal = claimsPrincipal;
            BusContext.Current = (CorrelationContext) CorrelationContext.Create<ICommand>(
                Trace.CorrelationManager.ActivityId,
                claimsPrincipal,
                Guid.Empty, String.Empty, null, String.Empty);
        }
    }
}