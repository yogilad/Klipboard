using System.Text;
using System.Text.RegularExpressions;
using System.IO.Compression;

using Microsoft.VisualBasic.FileIO;
using System.Diagnostics;
using System.Transactions;

namespace Klipboard.Utils
{
    #region Table Scheme Config
    public class TableScheme
    {
        public List<(string Name, KqlTypeDefinition Type)> Columns { get; private set; } = new List<(string, KqlTypeDefinition)>();

        public override string ToString()
        {
            var composedScheme = $"({string.Join(",", Columns.Select(c => $"['{c.Name}']:{c.Type.Name}"))})";
            
            return composedScheme;
        }
    }
    #endregion

    #region Column Findings
    internal class ColumnFindings
    {
        internal class ColumnFinding
        {
            public KqlTypeDefinition KqlTypeDef { get; }
            public int Count;

            public ColumnFinding(KqlTypeDefinition kqlType)
            {
                KqlTypeDef = kqlType;
            }
        }

        private static readonly Regex m_timespanRegex1 = new Regex("^[0-9]+[smhd]$", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        private static readonly Regex m_timespanRegex2 = new Regex("^\\s*(\\d+\\.)?\\d{2}:\\d{2}(:\\d{2}(\\.\\d+)?)?\\s*$", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        private static readonly KqlTypeDefinition s_stringDefinition = KqlTypeHelper.GetTypeDedfinition(KqlDataType.StringType);

        private List<ColumnFinding> m_matchers = new List<ColumnFinding>()
            {
                // Order is important - if a value is equally matched by multiple entries the first one wins
                new ColumnFinding(KqlTypeHelper.GetTypeDedfinition(KqlDataType.BoolType)),
                new ColumnFinding(KqlTypeHelper.GetTypeDedfinition(KqlDataType.LongType)),
                new ColumnFinding(KqlTypeHelper.GetTypeDedfinition(KqlDataType.RealType)),
                new ColumnFinding(KqlTypeHelper.GetTypeDedfinition(KqlDataType.TimeSpanType)),
                new ColumnFinding(KqlTypeHelper.GetTypeDedfinition(KqlDataType.DateTimeType)),
                new ColumnFinding(KqlTypeHelper.GetTypeDedfinition(KqlDataType.DynamicType)),
                new ColumnFinding(KqlTypeHelper.GetTypeDedfinition(KqlDataType.GuidType)),
                // We do not check strings since everything is essentialy a string 
            };

        public bool HasFindings => m_matchers.Sum(x => x.Count) > 0;
        
        public KqlTypeDefinition GetBestMatchColumnType()
        {
            var max = m_matchers.MaxBy(x => x.Count);

            return (max == null || max.Count == 0) ?  s_stringDefinition : max.KqlTypeDef;
        }

        public void AnalyzeField(string field)
        {
            for(int i = 0; i < m_matchers.Count; i++)
            {
                if (m_matchers[i].KqlTypeDef.IsMatch(field))
                {
                    m_matchers[i].Count++;
                }
            }
        }
    }
    #endregion

    #region Tabular Data Helper
    public static class TabularDataHelper
    {
        #region Public APIs
        public static bool TryDetectTabularTextFormatV2(string data, out char? separator)
        {
            // TODO implement separator detection
            separator = null;

            if (data[0] == '"')
            {
                for(int i = 1; i < data.Length; i++)
                {
                    if (data[i] == '"')
                    {
                        i++;
                        if (i == data.Length)
                        {
                            return false;
                        }

                        if (data[i] != '"')
                        {
                            separator = data[i];
                            return true;
                        }
                    }
                }
            }
            else
            {
                var stop = false;

                foreach(char c in data)
                {
                    switch(c)
                    {
                        case '\n':
                            stop = true;
                            break;

                        case '\t':
                        case ',':
                            separator = c;
                            stop = true;
                            break;
                    }

                    if (stop)
                    {
                        break;
                    }
                }
            }

            return separator != null;
        }

        public static bool TryAnalyzeTabularData(string tableData, string delimiter , out TableScheme scheme, out bool firstRowIsHeader)
        {
            using var stream = new MemoryStream(Encoding.UTF8.GetBytes(tableData));

            return TryAnalyzeTabularData(stream, delimiter, out scheme, out firstRowIsHeader);
        }

        public static bool TryAnalyzeTabularData(Stream inputStream, string delimiter, out TableScheme scheme, out bool firstRowIsHeader)
        {
            var parser = new TextFieldParser(inputStream)
            {
                Delimiters = new string[] { delimiter },
                HasFieldsEnclosedInQuotes = true,
            };

            return TryAnalyzeTabularData(parser, out scheme, out firstRowIsHeader);
        }

        public static bool TryConvertTableToInlineQuery(string tableData, string delimiter, out string inlineQuery)
        {
            inlineQuery = string.Empty;

            using var stream1 = new MemoryStream(Encoding.UTF8.GetBytes(tableData));
            
            if (!TryAnalyzeTabularData(stream1, delimiter, out var tableScheme, out var firstRowIsHeader))
            {
                return false;
            }

            using var stream2 = new MemoryStream(Encoding.UTF8.GetBytes(tableData));
            var builder = new StringBuilder();
            var parser = new TextFieldParser(stream2)
            {
                Delimiters = new string[] { delimiter },
                HasFieldsEnclosedInQuotes = true,
            };

            builder.Append("let Klipboard = datatable");
            builder.AppendLine(tableScheme.ToString());
            builder.AppendLine("[");

            if (firstRowIsHeader)
            {
                parser.ReadLine();
            }

            while(true)
            {
                var line = parser.ReadFields();
                if (line == null || line.Length == 0)
                {
                    break;
                }

                for (int i = 0; i < line.Length; i++)
                {
                    var parsedField = tableScheme.Columns[i].Type.InlineSerializeData(line[i]);
                    builder.Append(parsedField);
                    builder.Append(",");
                }

                builder.AppendLine("");
            }

            builder.AppendLine("];");
            builder.AppendLine("Klipboard");
            inlineQuery = builder.ToString();
            return true;
        }

        public static bool TryConvertFreeTextToInlineQuery(string freeText, out string inlineQuery)
        {
            using var stream = new MemoryStream(Encoding.UTF8.GetBytes(freeText));
            var streamReader = new StreamReader(stream);
            var queryBuilder = new StringBuilder();

            queryBuilder.AppendLine("let Klipboard = datatable(['Line']:string)");
            queryBuilder.AppendLine("[");

            while(!streamReader.EndOfStream)
            {
                var line = streamReader.ReadLine();

                if (line == null)
                {
                    continue;
                }

                var escapedLine = KqlTypeHelper.SerializeString(line);

                queryBuilder.Append(escapedLine);
                queryBuilder.AppendLine(",");
            }

            queryBuilder.AppendLine("];");
            queryBuilder.AppendLine("Klipboard");
            inlineQuery = queryBuilder.ToString();
            return true;
        }

        public static string GzipBase64(string inputString)
        {
            using (var outputStream = new MemoryStream())
            {
                byte[] inputBytes = Encoding.UTF8.GetBytes(inputString);

                using (var gZipStream = new GZipStream(outputStream, CompressionLevel.SmallestSize))
                {
                    gZipStream.Write(inputBytes, 0, inputBytes.Length);
                }

                var gzipBytes = outputStream.ToArray();
                var outputString = Convert.ToBase64String(gzipBytes);
                
                return outputString;
            }
        }

        #endregion

        #region Private APIs
        private static bool TryAnalyzeTabularData(TextFieldParser parser, out TableScheme scheme, out bool firstRowIsHeader)
        {
            scheme = new TableScheme();
            firstRowIsHeader = true;

            var firstRowfields = parser.ReadFields();

            if (firstRowfields == null || firstRowfields.Length == 0) 
            {
                return false;
            }

            var firstRowCols = new List<ColumnFindings>(firstRowfields.Length);

            for (int i = 0; i < firstRowfields.Length; i++)
            {
                firstRowCols.Add(new ColumnFindings());
                firstRowCols[i].AnalyzeField(firstRowfields[i]);
            }

            foreach (var col in firstRowCols) 
            {
                if (col.HasFindings)
                {
                    firstRowIsHeader = false;
                    break;
                }
            }

            var rowNum = 0;

            var cols = new List<ColumnFindings>(firstRowfields.Length);
            while (rowNum < 20)
            {
                var fields = parser.ReadFields();

                if (fields == null) 
                { 
                    break;
                }

                if (firstRowfields.Length != fields.Length)
                {
                    return false;
                }

                for(int i = 0; i < fields.Length; i++)
                {
                    if (rowNum == 0)
                    {
                        cols.Add(new ColumnFindings());
                    }

                    cols[i].AnalyzeField(fields[i]);
                }

                rowNum++;
            }

            var colsToProcess = cols;
            if (rowNum == 0)
            {
                firstRowIsHeader = false;
                colsToProcess = firstRowCols;
            }

            var colNo = 0;
            foreach(var col in colsToProcess)
            {
                scheme.Columns.Add((firstRowIsHeader ? firstRowfields[colNo] : $"Column_{colNo}", col.GetBestMatchColumnType()));
                colNo++;
            }
            return true;
        }
        #endregion

        #region old APIs who may not be necessary
        public static bool TryDetectTabularTextFormatV1(string data, out char seperator)
        {
            seperator = '\0';
            if (string.IsNullOrWhiteSpace(data))
            {
                return false;
            }

            int pos = 0;
            int lastTabs = 0;
            int lastComas = 0;

            while (ProcessNextLine(data, pos, out pos, out var lineLength, out var comas, out var tabs))
            {
                if (lineLength == 0)
                    continue;

                lastComas = (lastComas == 0) ? comas : lastComas;
                lastComas = (lastComas != comas) ? -1 : lastComas;

                lastTabs = (lastTabs == 0) ? tabs : lastTabs;
                lastTabs = (lastTabs != tabs) ? -1 : lastTabs;

                if (lastComas == -1 && lastTabs == -1)
                {
                    return false;
                }
            }

            if (lastTabs > 0)
            {
                seperator = '\t';
            }
            else if (lastComas > 0)
            {
                seperator = ',';
            }
            else
            {
                return false;
            }

            return true;
        }

        private static bool ProcessNextLine(string data, int pos, out int newPos, out int length, out int comas, out int tabs)
        {
            comas = 0;
            tabs = 0;
            length = 0;

            while (pos < data.Length)
            {
                var curChar = data[pos];
                switch (curChar)
                {
                    case '\t':
                        tabs++;
                        break;

                    case ',':
                        comas++;
                        break;
                }

                pos++;
                if (curChar != '\r')
                {
                    length++;
                }
                
                if (curChar == '\n')
                {
                    break;
                }
            }

            newPos = pos;
            return pos != data.Length;
        }
        #endregion
    }
    #endregion
}
