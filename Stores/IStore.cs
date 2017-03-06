using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cargo
{
    public interface IStore
    {
        Task ItemSaveAsync<T>(string key, T data, TimeSpan timeout, string secondaryKey = null);
        Task ItemSaveAsync(string key, byte[] data, TimeSpan timeout, string secondaryKey = null);
        Task<CacheItem<T>> TryItemRestoreAsync<T>(string key);
        Task<CacheItem<byte[]>> TryItemRestoreAsync(string key);
        Task<bool> ItemDeleteAsync(string key);
        Task<IEnumerable<string>> LookupKeysBySecondaryIndex(string secondaryKey);
        Task<IEnumerable<string>> GetAllKeys();
    }
}
