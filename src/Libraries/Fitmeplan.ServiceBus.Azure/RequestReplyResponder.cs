using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.ServiceBus.Core;
using Microsoft.Extensions.Logging;

namespace Fitmeplan.ServiceBus.Azure
{
    public class RequestReplyResponder
    {
        private readonly string _connectionString;
        readonly ISubscriptionClient _subscription;
        readonly Func<Message, Task<Message>> _responseFunction;
        private readonly ILogger _logger;

        public RequestReplyResponder(string connectionString, ISubscriptionClient subscription,
            Func<Message, Task<Message>> responseFunction, ILogger logger)
        {
            _connectionString = connectionString;
            _subscription = subscription;
            _responseFunction = responseFunction;
            _logger = logger;
        }

        public Task Run(CancellationToken token)
        {
            _subscription.RegisterMessageHandler(
                    (message, cancellationToken) => Respond(message, _responseFunction),
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
            return Task.CompletedTask;
        }

        private Task ExceptionReceivedHandler(ExceptionReceivedEventArgs exceptionReceivedEventArgs)
        {
            var ex = exceptionReceivedEventArgs.Exception;
            var context = exceptionReceivedEventArgs.ExceptionReceivedContext;

            _logger.LogError(ex, "ERROR handling message: {ExceptionMessage} - Context: {@ExceptionContext}", ex.Message, context);

            return Task.CompletedTask;
        }

        async Task Respond(Message request, Func<Message, Task<Message>> handleRequest)
        {
            // evaluate ReplyTo
            if (!string.IsNullOrEmpty(request.ReplyTo))
            {
                // now we're reasonably confident that the input message can be
                // replied to, so let's execute the message processing
                try
                {
                    // call the callback
                    var reply = await handleRequest(request);
                    // set the correlation-id on the reply 
                    reply.CorrelationId = request.MessageId;

                    var sender = new MessageSender(_connectionString, request.ReplyTo);
                    await sender.SendAsync(reply);
                }
                catch (Exception e)
                {
                    _logger.LogError(e, "Responder Error");
                    throw;
                }
                finally
                {
                    await _subscription.CompleteAsync(request.SystemProperties.LockToken);
                }
            }
        }
    }
}
