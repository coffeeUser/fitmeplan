using System;
using StackExchange.Redis;

namespace Fitmeplan.Storage.Redis
{
    public interface IRedisDataProvider
    {
        IDatabase GetDatabase(int db = -1);
        void Flush(int db);
        void Flush();
        void Subscribe(string channel, Action<RedisChannel, RedisValue> callback);
    }
}
