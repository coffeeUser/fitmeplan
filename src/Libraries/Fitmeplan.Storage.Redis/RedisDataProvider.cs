using System;
using StackExchange.Redis;

namespace Fitmeplan.Storage.Redis
{
    /// <summary>
    /// Represents Redis DataProvider over single database
    /// </summary>
    public class RedisDataProvider : IRedisDataProvider, IDisposable
    {
        private readonly ConnectionMultiplexer _connectionMultiplexer;

        public RedisDataProvider(string config)
        {
            //Change options here if necessary
            _connectionMultiplexer = ConnectionMultiplexer.Connect(ConfigurationOptions.Parse(config));
        }

        public IDatabase GetDatabase(int db = -1)
        {
            return _connectionMultiplexer.GetDatabase();
        }

        public void Flush(int db)
        {
            var endPoints = _connectionMultiplexer.GetEndPoints();
            foreach (var endPoint in endPoints)
            {
                _connectionMultiplexer.GetServer(endPoint).FlushDatabase(db);
            }
        }

        public void Flush()
        {
            var endPoints = _connectionMultiplexer.GetEndPoints();
            foreach (var endPoint in endPoints)
            {
                _connectionMultiplexer.GetServer(endPoint).FlushAllDatabases();
            }
        }

        public void Subscribe(string channel, Action<RedisChannel, RedisValue> callback)
        {
            _connectionMultiplexer.GetSubscriber().Subscribe(channel, callback);
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        /// <filterpriority>2</filterpriority>
        public void Dispose()
        {
            _connectionMultiplexer?.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}
