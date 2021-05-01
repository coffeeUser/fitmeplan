using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.ServiceBus.Core;
using Microsoft.Extensions.Logging;

namespace Fitmeplan.ServiceBus.Azure
{
    public class RequestReplySender
    {
        readonly ConcurrentDictionary<string, TaskCompletionSource<Message>> _pendingRequests =
            new ConcurrentDictionary<string, TaskCompletionSource<Message>>();

        readonly MessageReceiver _receiver;
        private readonly ILogger _logger;
        readonly ITopicClient _sender;

        public RequestReplySender(ITopicClient sender, MessageReceiver receiver, ILogger logger)
        {
            _sender = sender;
            _receiver = receiver;
            _logger = logger;

            _receiver.RegisterMessageHandler(async (message, token) => 
            {
                if (_pendingRequests.TryGetValue(message.CorrelationId, out var tc))
                {
                    tc.SetResult(message);
                }
                else
                {
                    // can't correlate, toss out
                    _logger.LogWarning($"Received response message with unknown CorrelationId: {message.CorrelationId}, Label: {message.Label}");
                }
                await Task.CompletedTask;
            }, ExceptionReceivedHandler);
        }

        public async Task<Message> Request(Message sendToSend, TimeSpan timeout)
        {
            if (string.IsNullOrWhiteSpace(sendToSend.MessageId))
            {
                throw new ArgumentException("Message must have a valid MessageId");
            }

            var tcs = new TaskCompletionSource<Message>();
            if (!_pendingRequests.TryAdd(sendToSend.MessageId, tcs))
            {
                throw new InvalidOperationException("Request with this MessageId is already pending");
            }
            var cts = new CancellationTokenSource(timeout);
            cts.Token.Register(() => tcs.TrySetCanceled());
            // If the cancellation token is triggered, we close the receiver, which will trigger 
            // the receive operation below to return null as the receiver closes.
            //cts.Token.Register(() => receiver.CloseAsync());

            await _sender.SendAsync(sendToSend);

            do
            {
                var reply = await tcs.Task;
                try
                {
                    _pendingRequests.TryRemove(reply.CorrelationId, out tcs);
                    return reply;
                }
                catch
                {
                    await _receiver.AbandonAsync(reply.SystemProperties.LockToken);
                    _pendingRequests.TryRemove(reply.CorrelationId, out tcs);
                    throw;
                }
            }
            while (!cts.IsCancellationRequested);
        }

        private Task ExceptionReceivedHandler(ExceptionReceivedEventArgs exceptionReceivedEventArgs)
        {
            var ex = exceptionReceivedEventArgs.Exception;
            var context = exceptionReceivedEventArgs.ExceptionReceivedContext;

            _logger.LogError(ex, "ERROR handling message: {ExceptionMessage} - Context: {@ExceptionContext}", ex.Message, context);

            return Task.CompletedTask;
        }
    }
}
