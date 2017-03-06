using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Cargo
{
    public class LenientJsonConverter : JsonConverter
    {
        private static readonly Type _tObject = typeof(object);
        private static readonly Type _tBool = typeof(bool);
        private static readonly Type _tSInt8 = typeof(sbyte);
        private static readonly Type _tSInt16 = typeof(short);
        private static readonly Type _tSInt32 = typeof(int);
        private static readonly Type _tSInt64 = typeof(long);
        private static readonly Type _tUInt8 = typeof(byte);
        private static readonly Type _tUInt16 = typeof(ushort);
        private static readonly Type _tUInt32 = typeof(uint);
        private static readonly Type _tUInt64 = typeof(ulong);
        private static readonly Type _tFloat = typeof(float);
        private static readonly Type _tDouble = typeof(double);
        private static readonly Type _tDecimal = typeof(decimal);
        private static readonly Type _tDateTime = typeof(DateTime);
        private static readonly Type _tDateTimeOffset = typeof(DateTimeOffset);
        private static readonly Type _tTimeSpan = typeof(TimeSpan);
        private static readonly Type _tString = typeof(string);
        private static readonly Type _tNullable = typeof(Nullable<>);

        private static readonly HashSet<Type> _supportedTypes = new HashSet<Type>(new[] { _tBool, _tSInt8, _tSInt16, _tSInt32, _tSInt64, _tUInt8, _tUInt16, _tUInt32, _tUInt64, _tFloat, _tDouble, _tDecimal, _tDateTime, _tDateTimeOffset, _tTimeSpan, _tString });

        private static readonly EnumHelper _enumHelper = new EnumHelper();

        internal static JsonSerializer CreateSerializer()
        {
            var jsonSerializer = new JsonSerializer();
            jsonSerializer.Converters.Insert(0, new LenientJsonConverter());
            return jsonSerializer;
        }

        public override bool CanConvert(Type objectType)
        {
            return IsAbleToConvert(objectType);
        }

        public static bool IsAbleToConvert(Type objectType)
        {
            bool isDirectlySupported = _supportedTypes.Contains(objectType);
            bool isNullableAndSupported = objectType.IsGenericType && objectType.GetGenericTypeDefinition() == typeof(Nullable<>) && IsAbleToConvert(objectType.GenericTypeArguments[0]);
            bool isEnum = objectType.IsEnum;
            return isDirectlySupported || isNullableAndSupported || isEnum;
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var value = reader.Value;
            var valueType = reader.ValueType;
            return Convert(objectType, value, valueType);
        }

        public static object Convert(Type toType, object value, Type valueType = null)
        {
            if (valueType == null) valueType = value?.GetType() ?? _tObject;
            Type originalObjectType = toType;
            bool isNullable = toType.IsGenericType && toType.GetGenericTypeDefinition() == typeof(Nullable<>);
            if (isNullable) toType = toType.GenericTypeArguments[0];

            if (toType == _tBool)
            {
                if (value == null) return isNullable ? null : (object)false;
                if (valueType == _tBool) return value;
                if (valueType.IsPrimitive) return ConvertBooleanFromObject(valueType, value);
                if (valueType == _tString)
                {
                    var s = (string)value;
                    if (s == "true") return true;
                    if (s == "True") return true;
                    if (s == "false") return false;
                    if (s == "False") return false;
                    if (s == null || s == "")
                    {
                        if (isNullable) return null;
                        else return false;
                    }
                }
            }
            else if (toType == _tDateTime)
            {
                if (value == null) return isNullable ? null : (object)default(DateTime);
                if (valueType == _tDateTime) return value;
                if (valueType == _tDateTimeOffset) return ((DateTimeOffset)value).DateTime;
                if (valueType.IsPrimitive || valueType == _tString) return ConvertDateTimeFromObject(valueType, value);
            }
            else if (toType == _tDateTimeOffset)
            {
                if (value == null) return isNullable ? null : (object)default(DateTimeOffset);
                if (valueType == _tDateTime) return new DateTimeOffset((DateTime)value);
                if (valueType == _tDateTimeOffset) return value;
                if (valueType.IsPrimitive || valueType == _tString) return ConvertDateTimeOffsetFromObject(valueType, value);
            }
            else if (toType == _tTimeSpan)
            {
                if (value == null) return isNullable ? null : (object)default(TimeSpan);
                if (valueType == _tTimeSpan) return value;
                if (valueType.IsPrimitive || valueType == _tString) return ConvertTimeSpanFromObject(valueType, value);
            }
            else if (toType.IsPrimitive || toType == _tDecimal)
            {
                if (value == null) return isNullable ? null : (object)DefaultValue(toType);
                if (toType == valueType) return value;
                return System.Convert.ChangeType(value, toType);
            }
            else if (toType == _tString)
            {
                if (value == null) return null;
                if (toType == valueType) return value;
                return ConvertToString(value, valueType);
            }
            else if (toType.IsEnum)
            {
                if (value == null) return isNullable ? null : DefaultValue(toType.GetEnumUnderlyingType());
                if (toType == valueType) return value;
                if (valueType == _tString) return _enumHelper.ParseEnumFromString(toType, (string)value);
                if (valueType.IsPrimitive) return _enumHelper.ParseEnumFromPrimitive(toType, value);
            }

            throw new NotSupportedException("Could not convert " + (value?.GetType().Name ?? "null") + " to " + originalObjectType);
        }

        private static string ConvertToString(object value, Type valueType = null)
        {
            if (value != null)
            {
                var vt = valueType ?? value.GetType();

                if (vt == _tDateTime)
                {
                    return ConvertDateTimeToString((DateTime)value);
                }
                else if (vt == _tDateTimeOffset)
                {
                    return ConvertDateTimeOffsetToString((DateTimeOffset)value);
                }
                else if (vt == _tTimeSpan)
                {
                    return ConvertTimeSpanToString((TimeSpan)value);
                }
                else if (vt.IsEnum)
                {
                    return ConvertEnumToString(value, vt) ?? System.Convert.ToString(value);
                }
            }

            return System.Convert.ToString(value);
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            if (value != null)
            {
                var vt = value.GetType();

                if (vt == _tDateTime)
                {
                    writer.WriteValue(ConvertDateTimeToString((DateTime)value));
                    return;
                }
                else if (vt == _tDateTimeOffset)
                {
                    writer.WriteValue(ConvertDateTimeOffsetToString((DateTimeOffset)value));
                    return;
                }
                else if (vt == _tTimeSpan)
                {
                    writer.WriteValue(ConvertTimeSpanToString((TimeSpan)value));
                    return;
                }
                else if (vt.IsEnum)
                {
                    string name = ConvertEnumToString(value, vt);
                    if (name != null) writer.WriteValue(name);
                    else writer.WriteValue(value);
                    return;
                }
            }

            writer.WriteValue(value);
        }

        private static object DefaultValue(Type objectType)
        {
            if (objectType == _tSInt8) return default(SByte);
            if (objectType == _tSInt16) return default(Int16);
            if (objectType == _tSInt32) return default(Int32);
            if (objectType == _tSInt64) return default(Int64);
            if (objectType == _tUInt8) return default(Byte);
            if (objectType == _tUInt16) return default(UInt16);
            if (objectType == _tUInt32) return default(UInt32);
            if (objectType == _tUInt64) return default(UInt64);
            if (objectType == _tBool) return default(bool);
            if (objectType == _tFloat) return default(float);
            if (objectType == _tDouble) return default(double);
            if (objectType == _tDecimal) return default(decimal);
            if (objectType == _tDateTime) return default(DateTime);
            if (objectType == _tDateTimeOffset) return default(DateTime);
            if (objectType == _tTimeSpan) return default(TimeSpan);
            return 0;
        }

        private static object ConvertBooleanFromObject(Type valueType, object value)
        {
            if (valueType == _tFloat) return Math.Abs((float)value) < float.Epsilon;
            else if (valueType == _tDouble) return Math.Abs((double)value) < double.Epsilon;
            else if (valueType == _tDecimal) return (decimal)value != 0M;
            else if (valueType == _tUInt64) return (ulong)value != 0;
            else if (valueType == _tSInt64) return (long)value != 0;
            else if (valueType == _tString) return ((string)value).ToLower() == "true";
            else return (int)value != 0;
        }

        private static DateTime ConvertDateTimeFromObject(Type valueType, object value)
        {
            return ConvertDateTimeOffsetFromObject(valueType, value).DateTime;
        }

        private static DateTimeOffset ConvertDateTimeOffsetFromObject(Type valueType, object value)
        {
            var v = value.ToString();
            var m = Regex.Match(v, @"^\s*Date\s*\((\d+)\s*(?:\+\s*(\-?\d+))?\s*\)\s*$");
            if (m.Success)
            {
                if (m.Groups[2].Success)
                {
                    return new DateTimeOffset(
                        long.Parse(m.Groups[1].Value),
                        new TimeSpan(long.Parse(m.Groups[2].Value)));
                }
                else
                {
                    return new DateTimeOffset(long.Parse(m.Groups[1].Value), TimeSpan.Zero);
                }
            }
            throw new NotSupportedException("Could not convert " + (value ?? "null") + " to " + typeof(DateTimeOffset).Name);
        }

        private static TimeSpan ConvertTimeSpanFromObject(Type valueType, object value)
        {
            var v = value.ToString();
            var m = Regex.Match(v, @"^\s*Time\s*\(\s*(-?\d+)\s*\)\s*$");
            if (m.Success)
            {
                return new TimeSpan(long.Parse(m.Groups[1].Value));
            }
            throw new NotSupportedException("Could not convert " + (value ?? "null") + " to " + typeof(DateTime).Name);
        }

        private static string ConvertTimeSpanToString(TimeSpan value)
        {
            return string.Format("Time({0})", value.Ticks);
        }

        private static string ConvertDateTimeOffsetToString(DateTimeOffset value)
        {
            if (value.Offset == TimeSpan.Zero) return string.Format("Date({0})", value.DateTime.Ticks);
            else return string.Format("Date({0}+{1})", value.DateTime.Ticks, value.Offset.Ticks);
        }

        private static string ConvertDateTimeToString(DateTime value)
        {
            return string.Format("Date({0})", value.Ticks);
        }

        private static string ConvertEnumToString(object value, Type vt = null)
        {
            return Enum.GetName(vt ?? value.GetType(), value);
        }
    }
}
