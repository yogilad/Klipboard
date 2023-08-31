using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Runtime.CompilerServices;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;


namespace Klipboard.Utils
{
    public enum KqlDataType
    {
        BoolType,
        LongType,
        RealType,
        TimeSpanType,
        DateTimeType,
        DynamicType,
        GuidType,
        StringType,
    }

    public class KqlTypeDefinition
    {
        public string Name {get;}
        public KqlDataType Type { get;}
        public string NullValue { get; }
        public Func<string, bool> IsMatch { get; }

        public KqlTypeDefinition(string name, KqlDataType type, string nullValue, Func<string, bool> valueMatcher)
        {
            Name = name;
            Type = type;
            NullValue = nullValue;
            IsMatch = valueMatcher;
        }
    }

    public static class KqlTypeHelper
    {
        private static readonly Regex s_timespanRegex1 = new Regex("^[0-9]+[smhd]$", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        private static readonly Regex s_timespanRegex2 = new Regex("^\\s*(\\d+\\.)?\\d{2}:\\d{2}(:\\d{2}(\\.\\d+)?)?\\s*$", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        private static readonly Dictionary<KqlDataType, KqlTypeDefinition> s_types = new Dictionary<KqlDataType, KqlTypeDefinition>()
            {
                // Order is important - if a value is equally matched by multiple entries the first one wins
                { KqlDataType.BoolType,     new KqlTypeDefinition("bool",       KqlDataType.BoolType,       "bool(null)",       s => bool.TryParse(s, out _))},
                { KqlDataType.LongType,     new KqlTypeDefinition("long",       KqlDataType.LongType,       "long(null)",       s => long.TryParse(s, out _))},
                { KqlDataType.RealType,     new KqlTypeDefinition("real",       KqlDataType.RealType,       "real(null)",       s => double.TryParse(s, out _))},
                { KqlDataType.TimeSpanType, new KqlTypeDefinition("timespan",   KqlDataType.TimeSpanType,   "timespan(null)",   s => IsMatchTimeSpan(s))},
                { KqlDataType.DateTimeType, new KqlTypeDefinition("datetime",   KqlDataType.DateTimeType,   "datetime(null)",   s => DateTime.TryParse(s, out _))},
                { KqlDataType.DynamicType,  new KqlTypeDefinition("dynamic",    KqlDataType.DynamicType,    "dynamic(null)",    s => IsMatchDynamic(s))},
                { KqlDataType.GuidType,     new KqlTypeDefinition("guid",       KqlDataType.GuidType,       "guid(null)",       s => Guid.TryParse(s, out _))},
                { KqlDataType.StringType,   new KqlTypeDefinition("string",     KqlDataType.StringType,     "\"\"",                 s => true)},
            };

        private static readonly Dictionary<KqlDataType, Func<string, string>> s_inlineSerializersInternal = new Dictionary<KqlDataType, Func<string, string>>()
            {
                // Order is important - if a value is equally matched by multiple entries the first one wins
                { KqlDataType.BoolType,    s => s},
                { KqlDataType.LongType,    s => SerializeLong(s)},
                { KqlDataType.RealType,    s => SerializeReal(s)},
                { KqlDataType.TimeSpanType,s => SerializeTimeSpan(s)},
                { KqlDataType.DateTimeType,s => SerializeDatetime(s)},
                { KqlDataType.DynamicType, s => SerializeDynamic(s)},
                { KqlDataType.GuidType,    s => SerializeGuid(s)},
                { KqlDataType.StringType,  s => SerializeString(s)},
            };

        public static KqlTypeDefinition GetTypeDedfinition(KqlDataType type) => s_types[type];

        private static bool IsMatchTimeSpan(string s)
        {
            return s_timespanRegex1.IsMatch(s) || s_timespanRegex2.IsMatch(s);
        }

        public static string InlineSerializeData(this KqlTypeDefinition typeDef, string field)
        {
            if (string.IsNullOrWhiteSpace(field) || field.Equals(typeDef.NullValue))
            {
                return typeDef.NullValue;
            }

            return s_inlineSerializersInternal[typeDef.Type](field);
        }

        private static bool IsMatchDynamic(string s)
        {
            s = s.Trim('"');
            return s.StartsWith("{") && s.EndsWith("}") || s.StartsWith("[") && s.EndsWith("]");
        }

        private static string SerializeDatetime(string field)
        {
            if (DateTime.TryParse(field, out var dt))
            {
                return $"datetime({dt.Year}-{dt.Month}-{dt.Day} {dt.Hour}:{dt.Minute}:{dt.Second}.{dt.Millisecond})";
            }

            return $"datetime({field})";
        }

        private static string SerializeTimeSpan(string field)
        {
            if (s_timespanRegex2.IsMatch(field))
            {
                return $"timespan({field})";
            }

            return field;
        }

        private static string SerializeDynamic(string field)
        {
            if (field.StartsWith("\"") && field.EndsWith("\"")) 
            {
                return field;
            }

            return $"\"{field.Replace("\"", "\\\"")}\"";
        }

        private static string SerializeString(string field)
        {
            return $"\"{field.Replace("\"", "\\\"")}\"";
        }

        private static string SerializeGuid(string field)
        {
            return $"\"{field}\"";
        }

        private static string SerializeReal(string field)
        {
            if(!field.Contains(","))
            {
                return field;
            }

            if (double.TryParse(field, out var r))
            {
                // Hoping this will not fail in wierd locales
                return r.ToString();
            }

            return field;
        }

        private static string SerializeLong(string field)
        {
            if (!field.Contains(","))
            {
                return field;
            }

            if (long.TryParse(field, out var l))
            {
                // Hoping this will not fail in wierd locales
                return l.ToString();
            }

            return field;
        }
    }
}
