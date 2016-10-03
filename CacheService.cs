//using Redis.ServerFarmTesting.Common;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace MHRD.Swayam.Common
{
    public interface ICacheService
    {
        bool IsAlive();
        bool Exists(string key);
        void Save(string key, string value, TimeSpan? expiry = null);
        void Save(string key, int value, TimeSpan? expiry = null);
        string Get(string key);
        List<string> Get(List<string> keys);
        void Remove(string key);
    }

    public class CacheService : ICacheService
    {
        private readonly IRedisCache _cache;
        private readonly TimeSpan _duration = TimeSpan.FromMilliseconds(5000);

        public CacheService()
        {
            var cacheEndpoint = ConfigurationManager.AppSettings["CacheEndpoint"];
            var cachePassword = ConfigurationManager.AppSettings["CachePassword"];
            var cacheRetry = 5;
            _cache = new RedisCache(cacheEndpoint, cachePassword, cacheRetry);
        }

        public bool IsAlive()
        {
            return _cache != null && _cache.IsAlive();
        }

        public void RunScript(string script)
        {
            _cache.DBInstance.ScriptEvaluate(script);
        }

        public bool Exists(string key)
        {
            return _cache.DBInstance.KeyExists(key);
        }

        public void Save(string key, string value, TimeSpan? expiry = null)
        {
            //_cache.DBInstance.StringSetAsync(key, value, expiry);
            _cache.DBInstance.StringSet(key, value, expiry);
            //cache.StringSet(key, SerializeJson(value), expiry: expiry);
        }
        public void Save(string key, object value, TimeSpan? expiry = null)
        {
            //_cache.DBInstance.StringSetAsync(key, value, expiry);
            //_cache.DBInstance.StringSet(key, value, expiry);
            _cache.DBInstance.StringSet(key, JsonConvert.SerializeObject(value), expiry: expiry);
        }

        public void Save(string key, int value, TimeSpan? expiry = null)
        {
            _cache.DBInstance.StringSet(key, value.ToString(), expiry);
        }

        public string Get(string key)
        {
            return _cache.DBInstance.StringGet(key);
        }
      

        public List<string> Get(List<string> keys)
        {
            List<RedisKey> hashedKeys = new List<RedisKey>();

            if (keys == null || keys.Count == 0)
                return null;

            foreach (string key in keys)
                hashedKeys.Add(key);

            return Get(hashedKeys);
        }

        public void Remove(string key)
        {
            _cache.DBInstance.KeyDelete(key);
        }

        #region Private Methods

        private List<string> Get(List<RedisKey> hashedRedisKeys)
        {
            var values = _cache.DBInstance.StringGet(hashedRedisKeys.ToArray()).Select(rv => rv.ToString()).ToList();

            // decrypt values
            int valuesCount = values.Count();
            for (int i = 0; i < valuesCount; i++)
                values[i] = values[i];

            return values;
        }

        #endregion
    }
}