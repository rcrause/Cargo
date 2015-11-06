using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Cargo
{
    internal class ResourceHelper
    {
        private Dictionary<string, string> _filesFromResource;
        private ReaderWriterLockSlim _rwl;

        public ResourceHelper()
        {
            _filesFromResource = new Dictionary<string, string>();
            _rwl = new ReaderWriterLockSlim();
        }

        public string this[string resource]
        {
            get
            {
                string data;
                _rwl.EnterUpgradeableReadLock();
                try
                {
                    if (!_filesFromResource.TryGetValue(resource, out data))
                    {
                        _rwl.EnterWriteLock();
                        try
                        {
                            if (!_filesFromResource.TryGetValue(resource, out data))
                            {
                                data = GetResourceInternal(resource);
                                _filesFromResource[resource] = data;
                            }
                        }
                        finally
                        {
                            _rwl.ExitWriteLock();
                        }
                    }
                }
                finally
                {
                    _rwl.ExitUpgradeableReadLock();
                }

                return data;
            }
        }

        private static string GetResourceInternal(string resourceName)
        {
            var assembly = typeof(CargoModule).Assembly;
            using (var stream = assembly.GetManifestResourceStream(resourceName))
            {
                using (StreamReader sr = new StreamReader(stream))
                {
                    return sr.ReadToEnd();
                }
            }
        }
    }
}
