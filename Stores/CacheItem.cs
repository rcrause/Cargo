using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cargo
{
    public class CacheItem<T>
    {
        public bool Success { get; set; }
        public T Item { get; set; }
        public DateTimeOffset? Expires { get; set; }
        public string Key { get; set; }
        public string SecondaryKey { get; set; }

        public TimeSpan GetExpiryTime()
        {
            if (Expires.HasValue)
            {
                return DateTimeOffset.UtcNow - Expires.Value.ToUniversalTime();
            }
            else
            {
                return TimeSpan.MaxValue;
            }
        }

        public static CacheItem<T> Empty(string forKey, string secondaryKey = null)
        {
            return new CacheItem<T> { Key = forKey, SecondaryKey = secondaryKey };
        }

        public static CacheItem<T> Cast<T2>(CacheItem<T2> other)
        {
            return new CacheItem<T>
            {
                Expires = other.Expires,
                Item = (T)(object)other.Item,
                Key = other.Key,
                SecondaryKey = other.SecondaryKey,
                Success = other.Success
            };
        }
    }
}
