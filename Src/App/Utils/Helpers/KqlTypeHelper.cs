using System.Text;
using System.Text.RegularExpressions;


namespace Klipboard.Utils
{
    public enum KqlDataType
    {
        BoolType,
        IntType,
        LongType,
        DecimalType,
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
        private static readonly Regex UnwantedRegex = new Regex("[\\s\"]", RegexOptions.Compiled);


        private static readonly Dictionary<string, KqlDataType> s_typeNames = new Dictionary<string, KqlDataType>(StringComparer.OrdinalIgnoreCase)
            {
                // Order is important - if a value is equally matched by multiple entries the first one wins
                { "bool",     KqlDataType.BoolType },
                { "int",      KqlDataType.IntType },
                { "long",     KqlDataType.LongType },
                { "decimal",  KqlDataType.DecimalType },
                { "real",     KqlDataType.RealType},
                { "timespan", KqlDataType.TimeSpanType },
                { "datetime", KqlDataType.DateTimeType },
                { "dynamic",  KqlDataType.DynamicType },
                { "guid",     KqlDataType.GuidType },
                { "string",   KqlDataType.StringType },
            };

        private static readonly Dictionary<KqlDataType, KqlTypeDefinition> s_typeDefintions = new Dictionary<KqlDataType, KqlTypeDefinition>()
            {
                // Order is important - if a value is equally matched by multiple entries the first one wins
                { KqlDataType.BoolType,     new KqlTypeDefinition("bool",       KqlDataType.BoolType,       "bool(null)",       s => bool.TryParse(s, out _))},
                { KqlDataType.IntType,      new KqlTypeDefinition("int",        KqlDataType.IntType,        "int(null)",        s => int.TryParse(s, out _))},
                { KqlDataType.LongType,     new KqlTypeDefinition("long",       KqlDataType.LongType,       "long(null)",       s => long.TryParse(s, out _))},
                { KqlDataType.DecimalType,  new KqlTypeDefinition("decimal",    KqlDataType.DecimalType,    "decimal(null)",    s => decimal.TryParse(s, out _))},
                { KqlDataType.RealType,     new KqlTypeDefinition("real",       KqlDataType.RealType,       "real(null)",       s => double.TryParse(s, out _))},
                { KqlDataType.TimeSpanType, new KqlTypeDefinition("timespan",   KqlDataType.TimeSpanType,   "timespan(null)",   s => IsMatchTimeSpan(s))},
                { KqlDataType.DateTimeType, new KqlTypeDefinition("datetime",   KqlDataType.DateTimeType,   "datetime(null)",   s => DateTime.TryParse(s, out _))},
                { KqlDataType.DynamicType,  new KqlTypeDefinition("dynamic",    KqlDataType.DynamicType,    "dynamic(null)",    s => IsMatchDynamic(s))},
                { KqlDataType.GuidType,     new KqlTypeDefinition("guid",       KqlDataType.GuidType,       "guid(null)",       s => Guid.TryParse(s, out _))},
                { KqlDataType.StringType,   new KqlTypeDefinition("string",     KqlDataType.StringType,     "\"\"",             s => true)},
            };

        private static readonly Dictionary<KqlDataType, Func<string, string>> s_inlineSerializersInternal = new Dictionary<KqlDataType, Func<string, string>>()
            {
                // Order is important - if a value is equally matched by multiple entries the first one wins
                { KqlDataType.BoolType,    s => s},
                { KqlDataType.IntType,     SerializeInt},
                { KqlDataType.LongType,    SerializeLong},
                { KqlDataType.DecimalType, SerializeDecimal},
                { KqlDataType.RealType,    SerializeReal},
                { KqlDataType.TimeSpanType,SerializeTimeSpan},
                { KqlDataType.DateTimeType,SerializeDatetime},
                { KqlDataType.DynamicType, SerializeDynamic},
                { KqlDataType.GuidType,    SerializeGuid},
                { KqlDataType.StringType,  SerializeString},
            };

        public static KqlTypeDefinition GetTypeDedfinition(KqlDataType type) => s_typeDefintions[type];
        public static bool TryGetTypeDedfinition(string typeName, out KqlTypeDefinition? typeDefintions)
        {
            if (s_typeNames.TryGetValue(typeName, out var type))
            {
                typeDefintions = s_typeDefintions[type];
                return true;
            }

            typeDefintions = null;
            return false;
        }

        public static bool IsMatchTimeSpan(string s)
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

        public static bool IsMatchDynamic(string s)
        {
            s = s.Trim('"');
            return s.StartsWith("{") && s.EndsWith("}") || s.StartsWith("[") && s.EndsWith("]");
        }

        public static string SerializeDatetime(string field)
        {
            if (DateTime.TryParse(field, out var dt))
            {
                return $"datetime({dt.Year}-{dt.Month}-{dt.Day} {dt.Hour}:{dt.Minute}:{dt.Second}.{dt.Millisecond})";
            }

            return $"datetime({field})";
        }

        public static string SerializeTimeSpan(string field)
        {
            if (s_timespanRegex2.IsMatch(field))
            {
                return $"timespan({field})";
            }

            return field;
        }

        public static string SerializeDynamic(string field)
        {
            if (field.StartsWith("\"") && field.EndsWith("\""))
            {
                return field;
            }

            return $"\"{field.Replace("\"", "\\\"")}\"";
        }

        private static string EscapeString(string input) {
            var literal = new StringBuilder(input.Length + 2);
            literal.Append('"');
            foreach (var c in input)
            {
                switch (c) {
                    case '\"': literal.Append("\\\""); break;
                    case '\\': literal.Append(@"\\"); break;
                    case '\0': literal.Append(@"\0"); break;
                    case '\a': literal.Append(@"\a"); break;
                    case '\b': literal.Append(@"\b"); break;
                    case '\f': literal.Append(@"\f"); break;
                    case '\n': literal.Append(@"\n"); break;
                    case '\r': literal.Append(@"\r"); break;
                    case '\t': literal.Append(@"\t"); break;
                    case '\v': literal.Append(@"\v"); break;
                    default: literal.Append(c);
                        break;
                }
            }
            literal.Append('"');
            return literal.ToString();
        }

        public static string SerializeString(string field)
        {
            return EscapeString(field);
        }

        public static string SerializeGuid(string field)
        {
            return $"\"{field}\"";
        }

        public static string SerializeReal(string field)
        {
            field = UnwantedRegex.Replace(field, "");

            if (double.TryParse(field, out var r))
            {
                // Hoping this will not fail in wierd locales
                return r.ToString();
            }

            return field;
        }

        public static string SerializeInt(string field)
        {
            field = UnwantedRegex.Replace(field, "");

            if (int.TryParse(field, out var i))
            {
                // Hoping this will not fail in wierd locales
                return i.ToString();
            }

            return field;
        }

        public static string SerializeLong(string field)
        {
            field = UnwantedRegex.Replace(field, "");

            if (long.TryParse(field, out var l))
            {
                // Hoping this will not fail in wierd locales
                return l.ToString();
            }

            return field;
        }

        public static string SerializeDecimal(string field)
        {
            field = UnwantedRegex.Replace(field, "");

            if (decimal.TryParse(field, out var d))
            {
                // Hoping this will not fail in wierd locales
                return d.ToString();
            }

            return field;
        }
    }
}
