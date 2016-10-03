﻿using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MHRD.Swayam.Common
{
    public interface IRedisCache
    {
        bool IsAlive();
        ConnectionMultiplexer Conn { get; }
        IDatabase DBInstance { get; }
    }

    internal class RedisCache : IRedisCache
    {
        protected static string _endpoint { get; set; }
        protected static string _password { get; set; }
        protected static int _retryCount { get; set; }

        protected static readonly Lazy<ConnectionMultiplexer> LazyConnection = new Lazy<ConnectionMultiplexer>(() =>
        {
            if (String.IsNullOrWhiteSpace(_endpoint) || String.IsNullOrWhiteSpace(_password))
                return null;

            ConfigurationOptions config = new ConfigurationOptions();
            config.EndPoints.Add(_endpoint);
            config.Password = _password;
            config.ConnectRetry = _retryCount; // retry connection if broken
            config.KeepAlive = 30; // keep connection alive (ping every minute)
            config.Ssl = true;
            config.SyncTimeout = 60000; // 60 seconds timeout for each get/set/remove operation
            config.ConnectTimeout = 300000; // 300 seconds to connect to the cache
            config.AbortOnConnectFail = false;
            config.AllowAdmin = true;
            return ConnectionMultiplexer.Connect(config);
        });

        public ConnectionMultiplexer Conn
        {
            get
            {
                return LazyConnection.Value;
            }
        }
        protected IDatabase _dbInstance { get; set; }
        public IDatabase DBInstance
        {
            get
            {
                return _dbInstance;
            }
        }

        public RedisCache(string endpoint, string password, int retryCount)
        {
            _endpoint = endpoint;
            _password = password;
            _retryCount = retryCount;

            if (Conn != null)
                _dbInstance = Conn.GetDatabase(0);
        }

        public bool IsAlive()
        {
            try
            {
                return this.Conn != null && this.DBInstance != null && this.DBInstance.Ping() != null;
            }
            catch (Exception ex)
            {
                return false;
            }
        }
    }
}