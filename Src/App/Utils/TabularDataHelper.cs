using System;
using System.IO;
using System.Windows;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using System.ComponentModel.DataAnnotations;

using Microsoft.VisualBasic;
using Microsoft.VisualBasic.FileIO;
using System.ComponentModel;
using System.Text.RegularExpressions;

namespace Klipboard.Utils
{
    public class TableScheme
    {
        public List<(string ColumnName, string ColumnType)> Columns { get; private set; } = new List<(string, string)>();

        public override string ToString()
        {
            var composedScheme = $"({string.Join(",", Columns.Select(c => $"['{c.ColumnName}']:{c.ColumnType}"))})";
            
            return composedScheme;
        }
    }

    internal class ColumnFindings
    {
        internal class ColumnFinding
        {
            public string KustoType { get; }
            public int Count;
            public Func<string, bool> IsMatch;

            public ColumnFinding(string kustoType, Func<string, bool> isMatch)
            {
                KustoType = kustoType;
                IsMatch = isMatch;
            }
        }

        private static readonly Regex m_timespanRegex = new Regex("^[0-9]+[smhd]$", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        private List<ColumnFinding> m_matchers = new List<ColumnFinding>()
            {
                new ColumnFinding("bool", s => bool.TryParse(s, out _)),
                new ColumnFinding("long", s => long.TryParse(s, out _)),
                new ColumnFinding("real", s => double.TryParse(s, out _)),
                new ColumnFinding("datetime", s => DateTime.TryParse(s, out _)),
                new ColumnFinding("timespan", s => m_timespanRegex.IsMatch(s)),
                new ColumnFinding("dynamic", s =>
                {
                    s = s.Trim('"');
                    return s.StartsWith("{") && s.EndsWith("}") || s.StartsWith("[") && s.EndsWith("]");
                }),
                new ColumnFinding("guid", s => Guid.TryParse(s, out _)),
            };

        public bool HasFindings => m_matchers.Sum(x => x.Count) > 0;
        
        public string GetBestMatchColumnType()
        {
            var max = m_matchers.MaxBy(x => x.Count);

            return max.Count == 0 ? "string" : max.KustoType;
        }

        public void AnalyzeField(string field)
        {
            for(int i = 0; i < m_matchers.Count; i++)
            {
                if (m_matchers[i].IsMatch(field))
                {
                    m_matchers[i].Count++;
                }
            }
        }
    }

    public static class TabularDataHelper
    {
        public static bool TryAnalyzeTabularData(string tableData, string delimiter , out TableScheme scheme, out bool firstRowIsHeader)
        {
            var stream = new MemoryStream(Encoding.UTF8.GetBytes(tableData));

            return TryAnalyzeTabularData(stream, delimiter, out scheme, out firstRowIsHeader);
        }

        public static bool TryAnalyzeTabularData(MemoryStream inputStream, string delimiter, out TableScheme scheme, out bool firstRowIsHeader)
        {
            var parser = new TextFieldParser(inputStream)
            {
                Delimiters = new string[] { delimiter },
                HasFieldsEnclosedInQuotes = true,
                
            };

            return TryAnalyzeTabularData(parser, out scheme, out firstRowIsHeader);
        }

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

        #region old APIs who may not be necessary
        public static bool TryDetectTabularTextFormat(string data, out char seperator)
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
}
