using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.ServiceBus.Core;
using Microsoft.Azure.ServiceBus.Management;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Fitmeplan.ServiceBus.Core;
using ServiceBusException = Microsoft.Azure.ServiceBus.ServiceBusException;

namespace Fitmeplan.ServiceBus.Azure
{
    public class AzureServiceBus : ServiceBusClientBase, IServiceBus, IDisposable
    {
        private class TopicState
        {
            public bool SubscriptionCreated { get; set; }
        }

        private readonly TimeSpan _requestTimeout;
        private readonly ManagementClient _managementClient;
        private readonly IContextAccessor _contextAccessor;
        private readonly ILogger<AzureServiceBus> _logger;
        private readonly ITransactionScope _transactionScope;
        private readonly ServiceBusConnectionStringBuilder _serviceBusConnectionStringBuilder;
        
        private readonly Dictionary<string, TopicClient> _subscriptions = new Dictionary<string, TopicClient>();
        private readonly Dictionary<string, TopicState> _topicsState = new Dictionary<string, TopicState>();
        private readonly ConcurrentDictionary<string, RequestReplySender> _senders = new ConcurrentDictionary<string, RequestReplySender>();

        /// <summary>
        /// Initializes a new instance of the <see cref="ServiceBusClientBase" /> class.
        /// </summary>
        /// <param name="settings">The settings.</param>
        /// <param name="contextAccessor">The context accessor.</param>
        /// <param name="logger">logger</param>
        /// <param name="transactionScope">The transaction scope.</param>
        public AzureServiceBus(ServiceBusConfiguration settings, IContextAccessor contextAccessor, ILogger<AzureServiceBus> logger, ITransactionScope transactionScope = null) : base(settings)
        {
            _managementClient = new ManagementClient(settings.ConnectionString);
            _contextAccessor = contextAccessor;
            _logger = logger;
            _transactionScope = transactionScope;
            _serviceBusConnectionStringBuilder = new ServiceBusConnectionStringBuilder(settings.ConnectionString);
            _requestTimeout = TimeSpan.FromSeconds(settings.RequestTimeoutInSeconds);
        }

        /// <summary>
        /// Publishes the asynchronous.
        /// </summary>
        /// <typeparam name="TRequest">The type of the request.</typeparam>
        /// <param name="request">The request.</param>
        /// <param name="context">The context.</param>
        /// <returns></returns>
        public async Task PublishAsync<TRequest>(TRequest request, ICorrelationContext context = null) where TRequest : class
        {
            var topicName = GetTopicName<TRequest>();

            await EnsureTopicSubscriptionCreatedAsync(topicName);

            var message = new Message
            {
                ContentType = "application/json",
                MessageId = Guid.NewGuid().ToString(),
                Body = request.AsBody(),
                Label = topicName
            };

            SetMessageContext(message);

            var topicClient = CreateModel(GetTopicName<TRequest>());
            await topicClient.SendAsync(message);
        }

        /// <summary>
        /// Requests the asynchronous.
        /// </summary>
        /// <typeparam name="TRequest">The type of the request.</typeparam>
        /// <typeparam name="TResponse">The type of the response.</typeparam>
        /// <param name="request">The request.</param>
        /// <param name="context">The context.</param>
        /// <returns></returns>
        public async Task<TResponse> RequestAsync<TRequest, TResponse>(TRequest request, ICorrelationContext context = null) where TRequest : class where TResponse : ResponseMessage, new()
        {
            var topicName = GetTopicName<TRequest>();
            var subscriptionName = GetSubscriptionName(topicName);

            await EnsureTopicSubscriptionCreatedAsync(topicName);

            var message = new Message
            {
                ContentType = "application/json",
                MessageId = Guid.NewGuid().ToString(),
                Body = request.AsBody(),
                Label = topicName,
                ReplyTo = GetReplyQueue(topicName),
                TimeToLive = TimeSpan.FromMinutes(5),
            };

            SetMessageContext(message);

            var requestReplySender = GetOrCreateSender<TRequest>(message.ReplyTo);
            var responseMessage = await requestReplySender.Request(message, _requestTimeout);
            var response = responseMessage.DeserializeMsg<TResponse>();
            return response;
        }

        /// <summary>
        /// Add subscription asynchronously.
        /// </summary>
        /// <typeparam name="TRequest">The type of the request.</typeparam>
        /// <param name="handler">The handler.</param>
        /// <returns></returns>
        public async Task SubscribeAsync<TRequest>(Func<TRequest, Task> handler) where TRequest : class
        {
            var topicName = GetTopicName<TRequest>();
            var subscriptionName = GetSubscriptionName(topicName);
            var endpoint = _serviceBusConnectionStringBuilder.GetNamespaceConnectionString();

            try
            {
                ISubscriptionClient subscriptionClient = new SubscriptionClient(endpoint, topicName, subscriptionName);
                await EnsureTopicSubscriptionCreatedAsync(topicName, subscriptionName);

                //_subscriptionClient.AddRuleAsync(new RuleDescription
                //{
                //    Filter = new CorrelationFilter {Label = eventName},
                //    Name = eventName
                //}).GetAwaiter().GetResult();

                subscriptionClient.RegisterMessageHandler(
                    async (message, token) =>
                    {
                        var request = message.DeserializeMsg<TRequest>();
                        var context = GetMessageContext(message);
                        // Complete the message so that it is not received again.
                        if (await TryHandleAsync(handler, request, context))
                        {
                            // Complete the message so that it is not received again.
                            // This can be done only if the subscriptionClient is created in ReceiveMode.PeekLock mode (which is the default).
                            await subscriptionClient.CompleteAsync(message.SystemProperties.LockToken);

                            // Note: Use the cancellationToken passed as necessary to determine if the subscriptionClient has already been closed.
                            // If subscriptionClient has already been closed, you can choose to not call CompleteAsync() or AbandonAsync() etc.
                            // to avoid unnecessary exceptions.
                        }
                    },
                    // Configure the message handler options in terms of exception handling, number of concurrent messages to deliver, etc.
                    new MessageHandlerOptions(ExceptionReceivedHandler)
                    {
                        // Maximum number of concurrent calls to the callback ProcessMessagesAsync(), set to 1 for simplicity.
                        // Set it according to how many messages the application wants to process in parallel.
                        MaxConcurrentCalls = 10, 
                        // Indicates whether MessagePump should automatically complete the messages after returning from User Callback.
                        // False below indicates the Complete will be handled by the User Callback as in `ProcessMessagesAsync` below.
                        AutoComplete = false
                    });
            }
            catch (ServiceBusException)
            {
                _logger.LogWarning("The messaging entity {eventName} already exists.", topicName);
            }
        }

        private async Task<bool> TryHandleAsync<TRequest>(Func<TRequest, Task> handler, TRequest request, CorrelationContext context) 
            where TRequest : class 
        {
            var processed = false;
            try
            {
                ApplyContext(request, context);
                _transactionScope?.BeginTran();
                await handler(request);
                _transactionScope?.Commit();

                processed = true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error on message processing");
                _transactionScope?.Rollback();
            }
            return processed;
        }

        /// <summary>
        /// Add subscription asynchronously.
        /// </summary>
        /// <typeparam name="TRequest">The type of the request.</typeparam>
        /// <typeparam name="TResponse">The type of the response.</typeparam>
        /// <param name="handler">The handler.</param>
        /// <returns></returns>
        public async Task SubscribeAsync<TRequest, TResponse>(Func<TRequest, Task<TResponse>> handler) where TRequest : class where TResponse : ResponseMessage, new()
        {
            var topicName = GetTopicName<TRequest>();
            var subscriptionName = GetSubscriptionName(topicName);
            var endpoint = _serviceBusConnectionStringBuilder.GetNamespaceConnectionString();

            try
            {
                await EnsureTopicSubscriptionCreatedAsync(topicName, subscriptionName, true);

                ISubscriptionClient subscriptionClient = new SubscriptionClient(endpoint, topicName, subscriptionName);

                var responder = new RequestReplyResponder(endpoint, subscriptionClient, async (message) =>
                {
                    var request = message.DeserializeMsg<TRequest>();
                    var context = GetMessageContext(message);
                    var response = await TryRespondAsync(handler, request, context);

                    var responseMessage = new Message
                    {
                        ContentType = "application/json",
                        MessageId = Guid.NewGuid().ToString(),
                        Body = response.AsBody(),
                        Label = topicName
                    };
                    return responseMessage;
                }, _logger);

                await responder.Run(new CancellationToken());
            }
            catch (ServiceBusException)
            {
                _logger.LogWarning("The messaging entity {eventName} already exists.", topicName);
            }
        }

        private async Task<TResponse> TryRespondAsync<TRequest, TResponse>(Func<TRequest, Task<TResponse>> handler,
            TRequest request, CorrelationContext context)
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

        /// <summary>
        /// Unsubscribes all.
        /// </summary>
        /// <exception cref="NotImplementedException"></exception>
        public void Unsubscribe()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Unsubscribes the specified handler.
        /// </summary>
        /// <typeparam name="TRequest">The type of the request.</typeparam>
        /// <param name="handler">The handler.</param>
        /// <exception cref="NotImplementedException"></exception>
        public void Unsubscribe<TRequest>(Action<TRequest> handler)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Unsubscribes the specified handler.
        /// </summary>
        /// <typeparam name="TRequest">The type of the request.</typeparam>
        /// <typeparam name="TResponse">The type of the response.</typeparam>
        /// <param name="handler">The handler.</param>
        /// <exception cref="NotImplementedException"></exception>
        public void Unsubscribe<TRequest, TResponse>(Func<TRequest, TResponse> handler)
        {
            throw new NotImplementedException();
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

        /// <summary>
        /// Use this handler to examine the exceptions received on the message pump.
        /// </summary>
        /// <param name="exceptionReceivedEventArgs">The <see cref="ExceptionReceivedEventArgs"/> instance containing the event data.</param>
        /// <returns></returns>
        private Task ExceptionReceivedHandler(ExceptionReceivedEventArgs exceptionReceivedEventArgs)
        {
            var ex = exceptionReceivedEventArgs.Exception;
            var context = exceptionReceivedEventArgs.ExceptionReceivedContext;

            _logger.LogError(ex, "ERROR handling message: {ExceptionMessage} - Context: {@ExceptionContext}", ex.Message, context);

            return Task.CompletedTask;
        }

        #region Context

        private void SetMessageContext(Message message)
        {
            var context = (CorrelationContext) CorrelationContext.Create<ICommand>(
                Trace.CorrelationManager.ActivityId,
                _contextAccessor.CurrentPrincipal, 
                Guid.Empty, string.Empty, null, string.Empty);
            message.UserProperties["MessageContext"] = JsonConvert.SerializeObject(context);
        }

        private CorrelationContext GetMessageContext(Message message)
        {
            if (message.UserProperties.ContainsKey("MessageContext"))
            {
                return JsonConvert.DeserializeObject<CorrelationContext>((string)message.UserProperties["MessageContext"]);
            }
            return new CorrelationContext();
        }

        private void ApplyContext<TRequest>(TRequest request, CorrelationContext context) where TRequest : class
        {
            if (request is MessageBase)
            {
                RestoreCurrentPrincipal(context);
                Trace.CorrelationManager.ActivityId = context.Id;
                BusContext.Current = context;
            }
        }

        #endregion

        #region help methods

        private RequestReplySender GetOrCreateSender<TRequest>(string replyTo) where TRequest : class
        {
            var topicName = GetTopicName<TRequest>();
            if (!_senders.TryGetValue(topicName, out var requestReplySender))
            {
                var endpoint = _serviceBusConnectionStringBuilder.GetNamespaceConnectionString();
                requestReplySender = new RequestReplySender(CreateModel(topicName), new MessageReceiver(endpoint, replyTo), _logger);
                _senders.TryAdd(topicName, requestReplySender);
            }
            return requestReplySender;
        }

        public ITopicClient CreateModel(string name)
        {
            var entityPath = name.ToLower();

            if (!_subscriptions.TryGetValue(entityPath, out var topicClient) || topicClient.IsClosedOrClosing)
            {
                topicClient = new TopicClient(_serviceBusConnectionStringBuilder.GetNamespaceConnectionString(), entityPath);
            }

            _subscriptions[entityPath] = topicClient;

            return topicClient;
        }

        private async Task EnsureTopicSubscriptionCreatedAsync(string topicName, string subscriptionName = null, bool reply = false)
        {
            if (!_topicsState.ContainsKey(topicName) && !await _managementClient.TopicExistsAsync(topicName))
            {
                await _managementClient.CreateTopicAsync(topicName);
            }
            _topicsState[topicName] = new TopicState();

            if (!string.IsNullOrEmpty(subscriptionName))
            {
                if (!_topicsState[topicName].SubscriptionCreated && !await _managementClient.SubscriptionExistsAsync(topicName, subscriptionName))
                {
                    await _managementClient.CreateSubscriptionAsync(topicName, subscriptionName);
                }

                _topicsState[topicName].SubscriptionCreated = true;
            }

            var replyQueue = GetReplyQueue(topicName);
            if (reply && !await _managementClient.QueueExistsAsync(replyQueue))
            {
                await _managementClient.CreateQueueAsync(replyQueue);
            }
        }

        private string GetSubscriptionName(string eventName)
        {
            return $"{Settings.ApplicationName}";
        }

        private static string GetTopicName<TRequest>() where TRequest : class
        {
            return typeof(TRequest).Name; //ToString().Replace("`1", string.Empty);
        }

        private static string GetReplyQueue(string topicName)
        {
            return $"{topicName}.Reply".ToLower();
        }

        #endregion

        /// <summary>Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.</summary>
        public void Dispose()
        {
            _managementClient.CloseAsync().ConfigureAwait(false).GetAwaiter().GetResult();
        }
    }
}
