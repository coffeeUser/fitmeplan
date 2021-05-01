using System;
using StackExchange.Redis;

namespace Fitmeplan.Storage.Redis
{
    public class LockoutService
    {
        private readonly IRedisDataProvider _dataProvider;
        private readonly TimeSpan _loginPeriod;
        private readonly int _loginAttempts;
        private readonly int _lockLengthInMinutes;

        /// <summary>
        /// Initializes a new instance of the <see cref="LockoutService" /> class.
        /// </summary>
        /// <param name="dataProvider">The redis data provider.</param>
        /// <param name="loginPeriod">The login period.</param>
        /// <param name="loginAttempts">The login attempts.</param>
        /// <param name="lockLengthInMinutes">The lock length in minutes.</param>
        public LockoutService(IRedisDataProvider dataProvider, TimeSpan loginPeriod, int loginAttempts, int lockLengthInMinutes)
        {
            _dataProvider = dataProvider;
            _loginPeriod = loginPeriod;
            _loginAttempts = loginAttempts;
            _lockLengthInMinutes = lockLengthInMinutes;
        }

        public int MaxFailedAccessAttemptsBeforeLockout
        {
            get { return _loginAttempts; }
        }

        public TimeSpan LoginPeriod
        {
            get { return _loginPeriod; }
        }

        public int LoginAttempts
        {
            get { return _loginAttempts; }
        }

        public int LockLengthInMinutes
        {
            get { return _lockLengthInMinutes; }
        }

        public long AccessFailed(string clientIp)
        {
            var db = _dataProvider.GetDatabase();

            var key = GetLoginAttemptKey(clientIp);

            var curValue = db.StringIncrement(key);

            if (curValue == 1)
            {
                db.KeyExpire(key, _loginPeriod);
            }

            if (curValue >= _loginAttempts)
            {
                LockClientIp(clientIp, "Too many login attempts", _lockLengthInMinutes);
                db.KeyDelete(key);
            }
            return curValue;
        }

        public long ResetPasswordFailed(string clientIp)
        {
            var db = _dataProvider.GetDatabase();

            var key = GetResetPasswordAttemptKey(clientIp);

            var curValue = db.StringIncrement(key);

            if (curValue == 1)
            {
                db.KeyExpire(key, _loginPeriod);
            }

            if (curValue >= _loginAttempts)
            {
                LockClientIp(clientIp, "Too many failed reset passwords attempts", _lockLengthInMinutes);
                db.KeyDelete(key);
            }
            return curValue;
        }

        private void LockClientIp(string clientIp, string reason, int lockLengthInMinutes)
        {
            var db = _dataProvider.GetDatabase();

            var key = GetBlockedIpKey(clientIp);

            db.StringSet(key, reason, TimeSpan.FromMinutes(lockLengthInMinutes));
        }

        private string GetBlockedIpKey(string clientIp)
        {
            return $"security:restrictions:block:ip:{clientIp}";
        }

        private RedisKey GetLoginAttemptKey(string clientIp)
        {
            return $"security:login:attempt:{clientIp}";
        }

        private RedisKey GetResetPasswordAttemptKey(string clientIp)
        {
            return $"security:reset:password:attempt:{clientIp}";
        }

        public bool IsLockedOut(string remoteIpAddress)
        {
            var db = _dataProvider.GetDatabase();

            var key = GetBlockedIpKey(remoteIpAddress);

            return db.KeyExists(key);
        }

        public void AccessSuccess(string clientIp)
        {
            var db = _dataProvider.GetDatabase();

            var key = GetLoginAttemptKey(clientIp);

            db.KeyDelete(key);
        }
    }
}
