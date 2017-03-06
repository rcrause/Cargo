using Newtonsoft.Json;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cargo
{
    public class RedisStore : IStore
    {
        private string _keyPrefix;
        private string _indexPrefix;
        private string _allIndexKey;

        private IDatabase _redis;
        private JsonSerializer _jsonSerializer;

        public JsonSerializer JsonSerializer { get { return _jsonSerializer; } set { _jsonSerializer = value; } }
        public string KeyPrefix { get { return _keyPrefix; } set { _keyPrefix = value; } }
        public string IndexPrefix { get { return _indexPrefix; } set { _indexPrefix = value; } }
        public IDatabase Database { get { return _redis; } set { _redis = value; } }

        public RedisStore(IDatabase redis, string keySpec)
        {
            _keyPrefix = $"{keySpec}:";
            _indexPrefix = $"{keySpec}-ix:";
            _allIndexKey = $"rs-ai-{keySpec}";

            _redis = redis;
            _jsonSerializer = LenientJsonConverter.CreateSerializer();
        }

        public virtual async Task<bool> ItemDeleteAsync(string key)
        {
            var cacheKey = _keyPrefix + key;
            var deleted = await _redis.KeyDeleteAsync(cacheKey);
            await _redis.HashDeleteAsync(_allIndexKey, key);

            return deleted;
        }

        public virtual async Task<CacheItem<T>> TryItemRestoreAsync<T>(string key)
        {
            return await ItemRestoreInternal<T>(key, true);
        }

        public virtual async Task<CacheItem<byte[]>> TryItemRestoreAsync(string key)
        {
            return await ItemRestoreInternal<byte[]>(key, false);
        }

        private async Task<CacheItem<T>> ItemRestoreInternal<T>(string key, bool deserialize)
        {
            var cacheKey = _keyPrefix + key;
            var val = await _redis.StringGetAsync(cacheKey);
            if (!val.IsNullOrEmpty)
            {
                T item;
                if (deserialize)
                {
                    item = DeserializeObject<T>(val);
                }
                else
                {
                    item = (T)(object)Encoding.UTF8.GetBytes(val);
                }
                var keyTtl = await _redis.KeyTimeToLiveAsync(key);
                var secKey = await GetSecondaryKey(key);
                return new CacheItem<T>
                {
                    Expires = keyTtl.HasValue ? DateTimeOffset.UtcNow + keyTtl.Value : (DateTimeOffset?)null,
                    Item = item,
                    Key = key,
                    Success = true,
                    SecondaryKey = secKey
                };
            }
            else
            {
                return CacheItem<T>.Empty(key);
            }
        }

        private async Task<string> GetSecondaryKey(string key)
        {
            RedisValue v = await _redis.HashGetAsync(_allIndexKey, key);
            if (v.IsNullOrEmpty) return null;
            else if (v == "0")
            {
                //legacy - we don't know the sec key
                return null;
            }
            else if (((string)v).StartsWith(":"))
            {
                var seckey = ((string)v).Substring(1);
                if (seckey.Length == 0) return null;
                else return seckey;
            }
            else
            {
                //no idea what's going on now
                return null;
            }
        }

        public async Task<IEnumerable<string>> GetAllKeys()
        {
            List<string> keys = new List<string>();
            var allkeys = await _redis.HashGetAllAsync(_allIndexKey);

            foreach (var key in allkeys.Select(x => (string)x.Name))
            {
                if (await _redis.KeyExistsAsync($"{KeyPrefix}{key}"))
                {
                    keys.Add(key);
                }
            }

            return keys;
        }

        public async Task<IEnumerable<string>> LookupKeysBySecondaryIndex(string secondaryKey)
        {
            var allHashItems = await _redis.HashKeysAsync(_indexPrefix + secondaryKey);
            var keysAndExists = await Task.WhenAll(allHashItems.Select(async key => new { key, exists = await _redis.KeyExistsAsync(_keyPrefix + key) }));
            return keysAndExists.Where(x => x.exists).Select(x => (string)x.key);
        }

        public virtual async Task ItemSaveAsync<T>(string key, T value, TimeSpan expiration, string secondaryKey)
        {
            await StoreItemInternal(key, value, expiration, secondaryKey, true);
        }

        public virtual async Task ItemSaveAsync(string key, byte[] value, TimeSpan expiration, string secondaryKey)
        {
            await StoreItemInternal(key, value, expiration, secondaryKey, false);
        }

        private async Task StoreItemInternal<T>(string key, T value, TimeSpan expiration, string secondaryKey, bool serialize)
        {
            var cacheKey = _keyPrefix + key;

            //save the item
            if (serialize)
            {
                await _redis.StringSetAsync(cacheKey, SerializeObject(value));
            }
            else
            {
                await _redis.StringSetAsync(cacheKey, (byte[])(object)value);
            }

            //save the secondary key
            if (!string.IsNullOrEmpty(secondaryKey))
            {
                await _redis.HashSetAsync(_indexPrefix + secondaryKey, key, 1);
            }

            //set things to expire
            if (expiration != TimeSpan.MaxValue)
            {
                await _redis.KeyExpireAsync(cacheKey, expiration, CommandFlags.FireAndForget);
                if (!string.IsNullOrEmpty(secondaryKey)) await _redis.KeyExpireAsync(_indexPrefix + secondaryKey, expiration, CommandFlags.FireAndForget);
            }

            //save in the index of all keys
            await _redis.HashSetAsync(_allIndexKey, key, $":{secondaryKey ?? ""}");
        }

        protected virtual string SerializeObject(object obj)
        {
            using (var sw = new StringWriter())
            {
                using (var jw = new JsonTextWriter(sw))
                {
                    _jsonSerializer.Serialize(jw, obj);
                    return sw.ToString();
                }
            }
        }

        protected virtual T DeserializeObject<T>(string serializedObject)
        {
            using (var sr = new StringReader(serializedObject))
            {
                using (var jr = new JsonTextReader(sr))
                {
                    return _jsonSerializer.Deserialize<T>(jr);
                }
            }
        }
    }
}
