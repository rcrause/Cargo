using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Cargo
{
    /// <summary>
    /// Some functionality for getting items from resource.
    /// </summary>
    internal class ResourceHelper
    {
        private Dictionary<string, string> _filesFromResource;
        private ReaderWriterLockSlim _rwl;
        private Assembly _assembly;
        private string _assemblyPrefix;

        public ResourceHelper()
        {
            _filesFromResource = new Dictionary<string, string>();
            _rwl = new ReaderWriterLockSlim();
            _assembly = typeof(ResourceHelper).Assembly;
            _assemblyPrefix = _assembly.GetName().Name;
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

        private string GetResourceInternal(string resourceName)
        {
            resourceName = $"{_assemblyPrefix}.{resourceName}";

            using (var stream = _assembly.GetManifestResourceStream(resourceName))
            {
                using (StreamReader sr = new StreamReader(stream))
                {
                    return sr.ReadToEnd();
                }
            }
        }
    }
}
