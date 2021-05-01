using System;
using System.Threading.Tasks;

namespace Fitmeplan.ServiceBus.Core
{
    public interface IServiceBus
    {
        /// <summary>Publishes the asynchronous.</summary>
        /// <typeparam name="TRequest">The type of the request.</typeparam>
        /// <param name="request">The request.</param>
        /// <param name="context">The context.</param>
        /// <returns></returns>
        Task PublishAsync<TRequest>(TRequest request, ICorrelationContext context = null)
            where TRequest : class;

        /// <summary>Requests the asynchronous.</summary>
        /// <typeparam name="TRequest">The type of the request.</typeparam>
        /// <typeparam name="TResponse">The type of the response.</typeparam>
        /// <param name="request">The request.</param>
        /// <param name="context">The context.</param>
        /// <returns></returns>
        Task<TResponse> RequestAsync<TRequest, TResponse>(TRequest request, ICorrelationContext context = null)
            where TRequest : class 
            where TResponse : ResponseMessage, new();

        /// <summary>
        /// Add subscription asynchronously.
        /// </summary>
        /// <typeparam name="TRequest">The type of the request.</typeparam>
        /// <param name="handler">The handler.</param>
        /// <returns></returns>
        Task SubscribeAsync<TRequest>(Func<TRequest, Task> handler)
            where TRequest : class;

        /// <summary>
        /// Add subscription asynchronously.
        /// </summary>
        /// <typeparam name="TRequest">The type of the request.</typeparam>
        /// <typeparam name="TResponse">The type of the response.</typeparam>
        /// <param name="handler">The handler.</param>
        /// <returns></returns>
        Task SubscribeAsync<TRequest, TResponse>(Func<TRequest, Task<TResponse>> handler) 
            where TRequest : class
            where TResponse : ResponseMessage, new();

        /// <summary>
        /// Unsubscribes all.
        /// </summary>
        void Unsubscribe();
        
        /// <summary>
        /// Unsubscribes the specified handler.
        /// </summary>
        /// <typeparam name="TRequest">The type of the request.</typeparam>
        /// <param name="handler">The handler.</param>
        void Unsubscribe<TRequest>(Action<TRequest> handler);

        /// <summary>
        /// Unsubscribes the specified handler.
        /// </summary>
        /// <typeparam name="TRequest">The type of the request.</typeparam>
        /// <typeparam name="TResponse">The type of the response.</typeparam>
        /// <param name="handler">The handler.</param>
        void Unsubscribe<TRequest, TResponse>(Func<TRequest, TResponse> handler);
    }

    public class SubscribeOptions
    {
        public bool Durable { get; set; }
        public SubscribeOptions SetDurable()
        {
            Durable = true;
            return this;
        }
        
        public bool AutoDelete { get; set; }
        public SubscribeOptions SetAutoDelete()
        {
            AutoDelete = true;
            return this;
        }

        public ushort PrefetchCount { get; set; } = 0;

        public SubscribeOptions SetPrefetchCount(ushort prefetchCount)
        {
            PrefetchCount = prefetchCount;
            return this;
        }

        public uint ConcurrencyLevel { get; set; } = 5;
        public SubscribeOptions SetConcurrencyLevel(uint concurrencyLevel)
        {
            ConcurrencyLevel = concurrencyLevel;
            return this;
        }
    }
}
