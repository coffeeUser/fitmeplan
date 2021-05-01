using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Exceptions;
using RawRabbit.Channel;
using RawRabbit.Configuration;

namespace Fitmeplan.ServiceBus.RawRabbit
{
    public class ChannelFactoryC : ChannelFactory
    {
        private readonly ServiceBusConfiguration _settings;
        private readonly ILogger<ChannelFactoryC> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="ChannelFactoryC" /> class.
        /// </summary>
        /// <param name="connectionFactory">The connection factory.</param>
        /// <param name="config">The configuration.</param>
        /// <param name="settings">The settings.</param>
        /// <param name="logger">The logger.</param>
        public ChannelFactoryC(IConnectionFactory connectionFactory, RawRabbitConfiguration config, ServiceBusConfiguration settings, ILogger<ChannelFactoryC> logger) 
            : base(connectionFactory, config)
        {
            _settings = settings;
            _logger = logger;
        }

        public override Task ConnectAsync(CancellationToken token = default(CancellationToken))
        {
            try
            {
                _logger.LogDebug($"Creating a new connection for {ClientConfig.Hostnames.Count} hosts.");
                Connection = ConnectionFactory.CreateConnection(ClientConfig.Hostnames, _settings.ApplicationName);
                Connection.ConnectionShutdown += (sender, args) =>
                    _logger.LogWarning($"Connection was shutdown by {args.Initiator}. ReplyText {args.ReplyText}");
            }
            catch (BrokerUnreachableException e)
            {
                _logger.LogInformation(e, "Unable to connect to broker");
                throw;
            }
            return Task.FromResult(true);
        }
    }
}
