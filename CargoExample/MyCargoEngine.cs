using Cargo;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;

namespace CargoExample
{
    public class MyCargoEngine : CargoEngine
    {
        public override bool AuthenticateRequest(IDictionary<string, object> environment)
        {
            return true;
        }

        public override ICargoDataSource CreateDataSource()
        {
            string configString = ConfigurationManager.AppSettings["Redis"];
            var options = ConfigurationOptions.Parse(configString);
            if (options.SyncTimeout < 10000) options.SyncTimeout = 10000;

            string redisPrefix = ConfigurationManager.AppSettings["redisCargoPrefix"];
            RedisStore cacheLayer = new RedisStore(ConnectionMultiplexer.Connect(options).GetDatabase(), redisPrefix);

            return new EntityFrameworkCargoDataSource(new MyDataContext(), cacheLayer);
        }
    }
}