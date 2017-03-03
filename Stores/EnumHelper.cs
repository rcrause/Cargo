using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Stores
{
    public class EnumHelper
    {
        private static Dictionary<Type, Dictionary<string, object>> _enumCache = new Dictionary<Type, Dictionary<string, object>>();
        private static readonly ReaderWriterLockSlim _rwl = new ReaderWriterLockSlim();

        public object ParseEnumFromString(Type enumType, string key)
        {
            _rwl.EnterUpgradeableReadLock();
            try
            {
                object value;
                Dictionary<string, object> values;

                if (_enumCache.TryGetValue(enumType, out values))
                {
                    if (values.TryGetValue(key, out value))
                    {
                        return value;
                    }
                    else
                    {
                        //there is a small likelihood that this lock is actually needed but do it just in case
                        _rwl.EnterWriteLock();
                        try
                        {
                            value = ParseEnumFromStringInternal(enumType, key);
                            if (value != null)
                            {
                                values[key] = value;
                            }

                            return value;
                        }
                        finally
                        {
                            _rwl.ExitWriteLock();
                        }
                    }
                }
                else
                {
                    //NOTE: there is a weird race condition here...see if you can spot it.
                    //Doen't affect output but performs suboptimally in rare situations.

                    //create the dictionary
                    values = CreateValuesDictionary(enumType);
                    if (!values.TryGetValue(key, out value))
                    {
                        value = ParseEnumFromStringInternal(enumType, key);
                        if (value != null)
                        {
                            values[key] = value;
                        }
                    }

                    if (value != null)
                    {
                        _rwl.EnterWriteLock();
                        try
                        {
                            _enumCache[enumType] = values;
                        }
                        finally
                        {
                            _rwl.ExitWriteLock();
                        }
                    }
                    return value;
                }
            }
            finally
            {
                _rwl.ExitUpgradeableReadLock();
            }
        }

        private Dictionary<string, object> CreateValuesDictionary(Type enumType)
        {
            string[] names = enumType.GetEnumNames();
            Array values = enumType.GetEnumValues();
            return names.Select((v, i) => new { v, i })
                .ToDictionary(x => x.v, x => values.GetValue(x.i));
        }

        private object ParseEnumFromStringInternal(Type enumType, string key)
        {
            int i;
            if (int.TryParse(key, out i))
            {
                return ParseEnumFromPrimitive(enumType, i);
            }
            else
            {
                return null;
            }
        }

        public object ParseEnumFromPrimitive(Type enumType, object value)
        {
            return Enum.Parse(enumType, value.ToString());
        }
    }
}
