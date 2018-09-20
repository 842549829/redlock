#region LICENSE

/*
 *   Copyright 2014 Angelo Simone Scotto <scotto.a@gmail.com>
 * 
 *   Licensed under the Apache License, Version 2.0 (the "License");
 *   you may not use this file except in compliance with the License.
 *   You may obtain a copy of the License at
 * 
 *       http://www.apache.org/licenses/LICENSE-2.0
 * 
 *   Unless required by applicable law or agreed to in writing, software
 *   distributed under the License is distributed on an "AS IS" BASIS,
 *   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 *   See the License for the specific language governing permissions and
 *   limitations under the License.
 * 
 * */

#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using StackExchange.Redis;

namespace Redlock.CSharp
{
    /// <summary>
    /// 分布式锁
    /// </summary>
    public class Redlock
    {
        private const double ClockDriveFactor = 0.01;

        /// <summary>
        /// String containing the Lua unlock script.
        /// </summary>
        private const string UnlockScript = @"
            if redis.call(""get"",KEYS[1]) == ARGV[1] then
                return redis.call(""del"",KEYS[1])
            else
                return 0
            end";

        /// <summary>
        /// 默认重试线程休眠算法时间戳
        /// </summary>
        private readonly TimeSpan DefaultRetryDelay = new TimeSpan(0, 0, 0, 0, 200);

        protected Dictionary<string, ConnectionMultiplexer> redisMasterDictionary = new Dictionary<string, ConnectionMultiplexer>();

        public Redlock(params ConnectionMultiplexer[] list)
        {
            foreach (var item in list)
            {
                redisMasterDictionary.Add(item.GetEndPoints().First().ToString(), item);
            }
        }

        /// <summary>
        /// 分布式锁策略 有一半以上锁定成功就算成功
        /// </summary>
        protected int Quorum => redisMasterDictionary.Count / 2 + 1;


        protected static byte[] CreateUniqueLockId()
        {
            return Guid.NewGuid().ToByteArray();
        }

        protected bool LockInstance(string redisServer, string resource, byte[] val, TimeSpan ttl)
        {
            bool succeeded;
            try
            {
                var redis = redisMasterDictionary[redisServer];
                succeeded = redis.GetDatabase().StringSet(resource, val, ttl, When.NotExists);
            }
            catch (Exception)
            {
                succeeded = false;
            }

            return succeeded;
        }

        protected void UnlockInstance(string redisServer, string resource, byte[] val)
        {
            RedisKey[] key = { resource };
            RedisValue[] values = { val };
            var redis = redisMasterDictionary[redisServer];
            redis.GetDatabase().ScriptEvaluate(UnlockScript, key, values);
        }

        /// <summary>
        /// 锁
        /// </summary>
        /// <param name="resource">key</param>
        /// <param name="timeSpan">锁的超时时间</param>
        /// <param name="defaultRetryCount">锁定失败重试次数</param>
        /// <returns>锁</returns>
        public Lock Lock(RedisKey resource, TimeSpan timeSpan, int defaultRetryCount = 3)
        {
            var val = CreateUniqueLockId();
            var innerLock = new Lock();
            var successfull = Retry(defaultRetryCount, DefaultRetryDelay, () =>
            {
                try
                {
                    var n = 0;
                    var startTime = DateTime.Now;

                    // Use keys
                    ForEachRedisRegistered(
                        redis =>
                        {
                            if (LockInstance(redis, resource, val, timeSpan))
                            {
                                n += 1;
                            }
                        }
                    );

                    /*
                     * Add 2 milliseconds to the drift to account for Redis expires
                     * precision, which is 1 millisecond, plus 1 millisecond min drift 
                     * for small TTLs.        
                     */
                    var drift = Convert.ToInt32(timeSpan.TotalMilliseconds * ClockDriveFactor + 2);
                    var validityTime = timeSpan - (DateTime.Now - startTime) - new TimeSpan(0, 0, 0, 0, drift);

                    if (n >= Quorum && validityTime.TotalMilliseconds > 0)
                    {
                        innerLock = new Lock(resource, val, validityTime);
                        return true;
                    }

                    ForEachRedisRegistered(
                        redis => { UnlockInstance(redis, resource, val); }
                    );
                    return false;
                }
                catch (Exception)
                {
                    return false;
                }
            });
            innerLock.IsLock = successfull;
            return innerLock;
        }

        protected void ForEachRedisRegistered(Action<string> action)
        {
            foreach (var item in redisMasterDictionary)
            {
                action(item.Key);
            }
        }

        protected bool Retry(int retryCount, TimeSpan retryDelay, Func<bool> action)
        {
            var maxRetryDelay = (int)retryDelay.TotalMilliseconds;
            var rnd = new Random();
            var currentRetry = 0;
            while (currentRetry++ < retryCount)
            {
                if (action())
                {
                    return true;
                }
                Thread.Sleep(rnd.Next(maxRetryDelay));
            }

            return false;
        }

        public void Unlock(Lock lockObject)
        {
            ForEachRedisRegistered(redis => { UnlockInstance(redis, lockObject.Resource, lockObject.Value); });
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.AppendLine(GetType().FullName);

            sb.AppendLine("Registered Connections:");
            foreach (var item in redisMasterDictionary) sb.AppendLine(item.Value.GetEndPoints().First().ToString());

            return sb.ToString();
        }
    }
}