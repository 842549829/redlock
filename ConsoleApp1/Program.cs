using System;
using Redlock.CSharp;
using StackExchange.Redis;

namespace ConsoleApp1
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            //127.0.0.1:6379：IP，端口
            //password：Redis密码
            //connectTimeout：连接超时时间，这里设置的是1000毫秒
            //connectRetry：重试连接次数
            //syncTimeout：同步操作默认超时时间
            //defaultDatabase:默认数据库
            //<add key="RedisTest"  WriteServer="127.0.0.1:6379,password=123456,connectTimeout=1000,connectRetry=1,syncTimeout=1000,defaultDatabase=15"/>
            var resourceName = "key";
            var dlm = new Redlock.CSharp.Redlock(
                ConnectionMultiplexer.Connect("127.0.0.1:6379,connectTimeout=1000,connectRetry=1,syncTimeout=1000,defaultDatabase=15"),
                ConnectionMultiplexer.Connect("192.168.100.142:6379,connectTimeout=1000,connectRetry=1,syncTimeout=1000,defaultDatabase=15"));

            Lock locked = null;
            try
            {
                locked = dlm.Lock(
                    resourceName,
                    new TimeSpan(0, 30, 10)
                );
            }
            finally
            {
                if (locked != null && locked.IsLock)
                {
                    dlm.Unlock(locked);
                }
            }
        }
    }
}