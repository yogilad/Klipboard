using System.Text;

namespace Test.Utils
{
    #region KustoType
    public enum KustoType
    {
        Bool_Type,
        Datetime_Type,
        Dynamic_Type,
        Guid_Type,
        Int_Type,
        Long_Type,
        Real_Type,
        String_Type,
        Timespan_Type,
        Decimal_Type
    }

    public static class ExtendedKustoType
    {
        private static readonly string[] s_typeNames = new string[10]
        {
            "bool",
            "datetime",
            "dynamic",
            "guid",
            "int",
            "long",
            "real",
            "string",
            "timespan",
            "decimal"
        };

        public static string AsString(this KustoType type)
        {
            return s_typeNames[(int)type];
        }
    }
    #endregion

    #region ColumnData
    public class ColumnData
    {
        public String Name { get; }
        public KustoType Type { get; }

        private static Random s_rand = new Random();

        private static readonly List<string> s_dynamics = new List<string>()
        {
            "[]",
            "{}",
            "[1 , 2]",
            "{\"name\":\"x\", \"num\":1}",
            "{\"obj\":{\"b\":true}}",
        };

        private static readonly List<string> s_strings = new List<string>()
        {
            "Hello World",
            "",
            "  ",
            "OMG this is awesome!",
            "ROFL",
        };

        private static readonly List<string> s_timespans = new List<string>()
        {
            "2s",
            "5m",
            "1h",
            "3d",
            "0d",
            "1.13:00",
            "14.13:00",
            "1.13:00:00",
            "14.13:00:00",
            "14.13:12:11.3",
            "13:12",
            "13:12:11",
            "13:12:11.3",
        };

        public ColumnData(string name, KustoType type)
        {
            Name = name;
            Type = type;
        }

        public string GenerateValue(bool escapeCommas = false)
        {
            switch(Type) 
            {
                case KustoType.Bool_Type:
                    var b = s_rand.Next(2);
                    return b == 0 ? "false" : "true";


        case KustoType.Datetime_Type:
                    var year = s_rand.Next(50) + 1970;
                    var mon = s_rand.Next(12) + 1;
                    var day = s_rand.Next(28) + 1;
                    var hour = s_rand.Next(24);
                    var min = s_rand.Next(60);
                    return $"{year}-{mon}-{day} {hour}:{min}";

                case KustoType.Dynamic_Type:
                    var dyn = s_rand.Next(s_dynamics.Count);
                    var synStr = s_dynamics[dyn];

                    if (escapeCommas && synStr.Contains(','))
                    {
                        synStr = synStr.Replace("\"", "\"\"");
                        synStr = $"\"{synStr}\"";
                    }

                    return synStr;

                case KustoType.Guid_Type:
                    return Guid.NewGuid().ToString();

                case KustoType.Int_Type:
                    var i = s_rand.Next(10000);
                    return i.ToString();

                case KustoType.Long_Type:
                    var l = s_rand.Next(100000000);
                    return l.ToString();

                case KustoType.Real_Type:
                    var r = (float) s_rand.Next(100000) / 1000;
                    return r.ToString();

                case KustoType.String_Type:
                    var str = s_rand.Next(s_strings.Count);
                    return s_strings[str];

                case KustoType.Timespan_Type:
                    var ts = s_rand.Next(s_timespans.Count);
                    return s_timespans[ts];

                case KustoType.Decimal_Type:
                    var dec = (double) s_rand.Next(10000000) / 10000;
                    return dec.ToString();

                default:
                    return string.Empty;
            }
        }

        public string GenerateNullValue()
        {
            return $"{Type.AsString()}(null)";
        }
    }
    #endregion

    #region TableGenerator
    public class TableGenerator
    {
        private List<ColumnData> m_columns = new List<ColumnData> ();
        
        public TableGenerator(bool autoGenerateScheme = false, bool addExtraColumns = false)
        {
            if (autoGenerateScheme)
            {
                AddColumn("Bool Column", KustoType.Bool_Type);
                AddColumn("DateTime Column", KustoType.Datetime_Type);
                AddColumn("Dynamic Column", KustoType.Dynamic_Type);
                AddColumn("Guid Column", KustoType.Guid_Type);
                AddColumn("Long Column", KustoType.Long_Type);
                AddColumn("Real Column", KustoType.Real_Type);
                AddColumn("String Column", KustoType.String_Type);
                AddColumn("TimeSpan Column", KustoType.Timespan_Type);

                if (addExtraColumns)
                {
                    AddColumn("Int Column", KustoType.Int_Type);
                    AddColumn("Decimal Column", KustoType.Decimal_Type);
                }
            }
        }

        public void AddColumn(string name, KustoType type)
        {
            m_columns.Add(new ColumnData(name, type));
        }

        public string GenerateTableString(int lines = 10, bool addHeader = true, bool addNullRows = false, bool addEmptyRows = false)
        {
            return GenerateData(lines, addHeader, addNullRows, addEmptyRows, '\t');
        }

        public MemoryStream GenerateTableStream(int lines = 10, bool addHeader = false, bool addNullRows = false, bool addEmptyRows = false)
        {
            var data = GenerateData(lines, addHeader, addNullRows, addEmptyRows, ',');
            return new MemoryStream(Encoding.UTF8.GetBytes(data));
        }

        public string GenerateTableScheme(bool firstRowIsHeader = true)
        {
            string listOfColumns;

            if (firstRowIsHeader)
            {
                listOfColumns = string.Join(",", m_columns.Select(c => $"['{c.Name}']:{c.Type.AsString()}"));
            }
            else
            {
                var first = true;
                var i = 0;
                var builder = new StringBuilder();

                foreach (var col in m_columns)
                {
                    if (first)
                    {
                        first = false;
                    }
                    else
                    {
                        builder.Append(",");
                    }

                    builder.Append($"['Column_{i}']:{col.Type.AsString()}");
                    i++;
                }

                listOfColumns = builder.ToString();
            }

            return $"({listOfColumns})";
        }

        private string GenerateData(int lines, bool addHeader, bool addNullRows, bool addEmptyRows, char seperator)
        {
            var builder = new StringBuilder();
            var escapeCommas = seperator == ',';

            if (addHeader)
            {
                builder.Append(string.Join(seperator, m_columns.Select(c => c.Name)));
                builder.Append(Environment.NewLine);
            }
            
            if (addEmptyRows)
            {
                lines--;
                builder.Append(string.Join(seperator, m_columns.Select(c => "")));
                builder.Append(Environment.NewLine);
            }

            if (addNullRows)
            {
                lines--;
                builder.Append(string.Join(seperator, m_columns.Select(c => c.GenerateNullValue())));
                builder.Append(Environment.NewLine);
            }

            while (lines > 0)
            {
                lines--;
                builder.Append(string.Join(seperator, m_columns.Select(c => c.GenerateValue(escapeCommas))));
                builder.Append(Environment.NewLine);
            }

            return builder.ToString();
        }
    }
    #endregion
}