using System;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using RawRabbit;
using RawRabbit.Common;
using RawRabbit.Enrichers.MessageContext;
using RawRabbit.Operations.Respond.Context;
using RawRabbit.Operations.Subscribe.Context;
using RawRabbit.Pipe;
using Fitmeplan.ServiceBus.Core;

namespace Fitmeplan.ServiceBus.RawRabbit
{
    public class RawRabbitServiceBus : ServiceBusClientBase, IServiceBus
    {
        private readonly IBusClient _busClient;
        private readonly ILogger<RawRabbitServiceBus> _logger;
        private readonly ITransactionScope _transactionScope;

        /// <summary>
        /// Initializes a new instance of the <see cref="RawRabbitServiceBus" /> class.
        /// </summary>
        /// <param name="settings">The settings.</param>
        /// <param name="busClient">The bus client.</param>
        /// <param name="logger">The logger.</param>
        /// <param name="transactionScope">The transaction scope.</param>
        public RawRabbitServiceBus(ServiceBusConfiguration settings, IBusClient busClient, ILogger<RawRabbitServiceBus> logger, ITransactionScope transactionScope = null)
            : base(settings)
        {
            _busClient = busClient;
            _logger = logger;
            _transactionScope = transactionScope;
        }

        private SubscribeOptions GetDefaultSubscribeOptions<T>()
        {
            var type = typeof(T);
            SubscribeOptions options;
            //if (Settings.Commands?.ContainsKey(type.Name) ?? false)
            //{
            //    options = Settings.Commands[type.Name];
            //}
            //else
            {
                options = new SubscribeOptions();
            }

            var attribute = type.GetCustomAttribute<MessageSettingsAttribute>();
            if (attribute != null)
            {
                options.Durable = attribute.Durable;
                options.AutoDelete = attribute.AutoDelete;
            }

            //options.ConcurrencyLevel = Settings.Concurrency;
            return options;
        }
        
        public Task PublishAsync<TRequest>(TRequest request, ICorrelationContext context = null) 
            where TRequest : class
        {
            SubscribeOptions options = GetDefaultSubscribeOptions<TRequest>();

            return _busClient.PublishAsync(request, ctx =>
            {
                if (context != null)
                {
                    ctx.UseMessageContext(context);
                }
                ctx.UsePublishConfiguration(cfg => cfg.OnDeclaredExchange(e =>
                {
                    e.WithDurability(options.Durable);
                    e.WithAutoDelete(options.AutoDelete);
                }));
            });
        }

        public virtual async Task<TResponse> RequestAsync<TRequest, TResponse>(TRequest request, ICorrelationContext context = null)
            where TRequest : class 
            where TResponse : ResponseMessage, new()
        {
            var result = await _busClient.RequestAsync<TRequest, TResponse>(request, ctx =>
            {
                if (context != null)
                {
                    ctx.UseMessageContext(context);
                }
            });
            if (result.Exception != null)
            {
                _logger.LogError(result.Exception.Message);
            }
            return result;
        }

        public async Task SubscribeAsync<TRequest>(Func<TRequest, Task> handler) 
            where TRequest : class 
        {
            SubscribeOptions options = GetDefaultSubscribeOptions<TRequest>();

            await _busClient.SubscribeAsync<TRequest, CorrelationContext>(async (request, context) =>
            {
                var acknowledgement = await TryHandleAsync(handler, request, context);

                return acknowledgement;
            }, ApplySubscribeConfiguration(options));
        }

        private async Task<Acknowledgement> TryHandleAsync<TRequest>(Func<TRequest, Task> handler, TRequest request, CorrelationContext context) 
            where TRequest : class 
        {
            try
            {
                ApplyContext(request, context);
                _transactionScope?.BeginTran();
                await handler(request);
                _transactionScope?.Commit();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error on message processing");
                _transactionScope?.Rollback();
            }
            return new Ack();
        }

        public async Task SubscribeAsync<TRequest, TResponse>(Func<TRequest, Task<TResponse>> handler) 
            where TRequest : class 
            where TResponse : ResponseMessage, new()
        {
            SubscribeOptions options = GetDefaultSubscribeOptions<TRequest>();

            await _busClient.RespondAsync<TRequest, TResponse, CorrelationContext>(async (request, context) =>
            {
                var response = await TryRespondAsync(handler, request, context);
                return response;
            }, ApplyRespondConfiguration(options));
        }

        private async Task<TResponse> TryRespondAsync<TRequest, TResponse>(Func<TRequest, Task<TResponse>> handler,
            TRequest request, ICorrelationContext context)
            where TRequest : class 
            where TResponse : ResponseMessage, new()
        {
            var response = new TResponse();
            try
            {
                ApplyContext(request, context);
                _transactionScope?.BeginTran();
                response = await handler(request);
                _transactionScope?.Commit();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error on message processing");
                _transactionScope?.Rollback();
                response.Exception = CreateExceptionInformation<TRequest>(ex.UnwrapInnerException(), context);
            }
            
            return response;
        }

        private static Action<ISubscribeContext> ApplySubscribeConfiguration(SubscribeOptions options)
        {
            return ctx => ctx
                .UseSubscribeConfiguration(cfg => cfg
                    .Consume(c => c.WithPrefetchCount(options.PrefetchCount))
                    .FromDeclaredQueue(q => q
                        .WithAutoDelete(options.AutoDelete)
                        .WithDurability(options.Durable))
                    .OnDeclaredExchange(e => e.WithDurability(options.Durable))
                )
                .UseConsumerConcurrency(options.ConcurrencyLevel);
        }

        private static Action<IRespondContext> ApplyRespondConfiguration(SubscribeOptions options)
        {
            return ctx => ctx
                .UseRespondConfiguration(cfg => cfg
                    .Consume(c => c.WithPrefetchCount(options.PrefetchCount))
                    .FromDeclaredQueue(q => q
                        .WithAutoDelete(options.AutoDelete)
                        .WithDurability(options.Durable))
                    .OnDeclaredExchange(e => e.WithDurability(options.Durable))
                )
                .UseConsumerConcurrency(options.ConcurrencyLevel);
        }

        private void ApplyContext<TRequest>(TRequest request, ICorrelationContext context) where TRequest : class
        {
            if (request is MessageBase)
            {
                RestoreCurrentPrincipal(context);
                Trace.CorrelationManager.ActivityId = context.Id;
                BusContext.Current = context;
            }
        }

        public void Unsubscribe()
        {
        }

        public void Unsubscribe<TRequest>(Action<TRequest> handler)
        {
            
        }

        public void Unsubscribe<TRequest, TResponse>(Func<TRequest, TResponse> handler)
        {
            
        }

        protected static ExceptionInformation CreateExceptionInformation<TRequest>(Exception exception, ICorrelationContext context)
        {
            return new ExceptionInformation
            {
                Message = $"An unhandled exception was thrown when consuming a message\n  MessageId: {context.Id}\n  Request: '{typeof(TRequest).Name}'\nSee inner exception for more details.",
                ExceptionType = exception.GetType().FullName,
                StackTrace = exception.StackTrace,
                InnerMessage = exception.Message
            };
        }
    }
}