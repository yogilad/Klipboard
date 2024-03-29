﻿using System.Text;
using System.Text.RegularExpressions;
using System.IO.Compression;
using Microsoft.VisualBasic.FileIO;
using Kusto.Cloud.Platform.Utils;
using Kusto.Data.Common;
using Kusto.Ingest;
using Azure.Storage.Blobs.Models;

namespace Klipboard.Utils
{
    #region KqlTypeDetectionMode
    public enum KqlTypeDetectionMode
    {
        OnlyStrings,
        StringsAndNumbers,
        StringsNumbersAndTime,
        StringsNumbersTimeAndComplexTypes,
        All
    }
    #endregion

    #region Table Columns
    public class TableColumns
    {
        private static char[] s_allowedSigns = { ' ', '-', '_', '.'};
        public List<(string Name, KqlTypeDefinition Type)> Columns { get; private set; } = new List<(string, KqlTypeDefinition)>();

        private static string EscapeColumnName(string columnName, int columnIndex = 0)
        {
            columnName = columnName.Trim();

            if ((columnName.StartsWith("['") || columnName.StartsWith("[\"")) &&
                (columnName.EndsWith("']") || columnName.EndsWith("\"]")))
            {
                return columnName;
            }

            if (string.IsNullOrWhiteSpace(columnName))
            {
                return $"['Column_{columnIndex}']";
            }

            columnName = columnName.Replace("'", "\\'");
            return $"['{columnName}']";
        }

        private static string NormalizeColumnName(string columnName, int columnIndex = 0)
        {
            columnName = columnName.Trim();

            if ((columnName.StartsWith("['") || columnName.StartsWith("[\"")) &&
                (columnName.EndsWith("']") || columnName.EndsWith("\"]")))
            {
                columnName = columnName.Substring(2, columnName.Length - 4);
            }

            if (string.IsNullOrWhiteSpace(columnName))
            {
                return $"['Column_{columnIndex}']";
            }

            var nameBuilder = new StringBuilder();

            nameBuilder.Append("['");

            foreach (var c in  columnName) 
            {
                if ((((int) c) > 127) ||
                    (c >= 'a' && c <= 'z') ||
                    (c >= 'A' && c <= 'Z') ||
                    (c >= '0' && c <= '9') ||
                    s_allowedSigns.Contains(c))
                {
                    nameBuilder.Append(c);
                }
                else 
                {
                    nameBuilder.Append('_');
                }
            }
            nameBuilder.Append("']");

            return nameBuilder.ToString();
        }

        private static string StrippedColumnName(string columnName)
        {
            columnName = columnName.Trim();

            if ((columnName.StartsWith("['") || columnName.StartsWith("[\"")) &&
                (columnName.EndsWith("']") || columnName.EndsWith("\"]")))
            {
                columnName = columnName.Substring(2, columnName.Length - 4);
            }

            return columnName;
        }

        private static string NormalizeTypeName(KqlTypeDefinition typeDefinition, KqlTypeDetectionMode detectionMode)
        {
            switch (typeDefinition.Type)
            {
                case KqlDataType.BoolType:
                case KqlDataType.IntType:
                case KqlDataType.LongType:
                case KqlDataType.DecimalType:
                case KqlDataType.RealType:
                    if (detectionMode >= KqlTypeDetectionMode.StringsAndNumbers)
                        return typeDefinition.Name;

                    break;

                case KqlDataType.TimeSpanType:
                case KqlDataType.DateTimeType:
                    if (detectionMode >= KqlTypeDetectionMode.StringsNumbersAndTime)
                        return typeDefinition.Name;

                    break;

                case KqlDataType.DynamicType:
                case KqlDataType.GuidType:
                    if (detectionMode >= KqlTypeDetectionMode.StringsNumbersTimeAndComplexTypes)
                        return typeDefinition.Name;

                    break;
            }

            return KqlTypeHelper.GetTypeDedfinition(KqlDataType.StringType).Name;
        }

        public override string ToString()
        {
            return ToSchemaString(KqlTypeDetectionMode.All, strictEntityNaming: false);
        }

        public string ToSchemaString(KqlTypeDetectionMode detectionMode, bool strictEntityNaming = false)
        {
            var schemaBuilder = new StringBuilder();
            var notFirstCol = false;

            schemaBuilder.Append("(");
            for (int i = 0; i < Columns.Count; i++)
            {
                var columnName = Columns[i].Name;
                var columnType = Columns[i].Type;

                if (notFirstCol)
                {
                    schemaBuilder.Append(", ");
                }

                if(strictEntityNaming)
                {
                    columnName = NormalizeColumnName(Columns[i].Name, i);
                }
                else
                {
                    columnName = EscapeColumnName(Columns[i].Name, i);
                }

                schemaBuilder.Append(columnName);

                schemaBuilder.Append(":");
                schemaBuilder.Append(NormalizeTypeName(columnType, detectionMode));
                notFirstCol = true;
            }

            schemaBuilder.Append(")");
            return schemaBuilder.ToString();
        }

        public IngestionMapping ToJsonMapping()
        {
            var mappingList = new List<ColumnMapping>();

            for (int i = 0; i < Columns.Count; i++)
            {
                var col = Columns[i];
                var colMapping = new ColumnMapping(StrippedColumnName(NormalizeColumnName(col.Name, i)), 
                    col.Type.Name, 
                    new Dictionary<string, string>() { {"Path", $"$.{StrippedColumnName(col.Name)}"} });

                mappingList.Add(colMapping);
            }

            var mapping = new IngestionMapping()
            {
                IngestionMappingKind = Kusto.Data.Ingestion.IngestionMappingKind.Json,
                IngestionMappings = mappingList,
            };

            return mapping;
        }
    }
    #endregion

    #region Column Findings
    internal class ColumnFindings
    {
        internal class ColumnFinding
        {
            public KqlTypeDefinition KqlTypeDef { get; }
            public int MatchCount;
            public int MismatchCount;


            public ColumnFinding(KqlTypeDefinition kqlType)
            {
                KqlTypeDef = kqlType;
            }
        }

        private static readonly Regex m_timespanRegex1 = new Regex("^[0-9]+[smhd]$", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        private static readonly Regex m_timespanRegex2 = new Regex("^\\s*(\\d+\\.)?\\d{2}:\\d{2}(:\\d{2}(\\.\\d+)?)?\\s*$", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        private static readonly KqlTypeDefinition s_stringDefinition = KqlTypeHelper.GetTypeDedfinition(KqlDataType.StringType);

        private readonly List<ColumnFinding> m_matchers;

        internal ColumnFindings(KqlTypeDetectionMode detectionMode)
        {
            m_matchers = new List<ColumnFinding>();

            // Order is important - if a value is equally matched by multiple entries the first one wins
            // We do not check strings since everything is essentialy a string 

            if (detectionMode >= KqlTypeDetectionMode.StringsAndNumbers)
            {
                // We do not check int due to duplication with long
                // We do not check decimal due to duplication with real (even though it's range is higher)
                m_matchers.Add(new ColumnFinding(KqlTypeHelper.GetTypeDedfinition(KqlDataType.BoolType)));
                m_matchers.Add(new ColumnFinding(KqlTypeHelper.GetTypeDedfinition(KqlDataType.LongType)));
                m_matchers.Add(new ColumnFinding(KqlTypeHelper.GetTypeDedfinition(KqlDataType.RealType)));
            }

            if (detectionMode >= KqlTypeDetectionMode.StringsNumbersAndTime)
            {
                m_matchers.Add(new ColumnFinding(KqlTypeHelper.GetTypeDedfinition(KqlDataType.TimeSpanType)));
                m_matchers.Add(new ColumnFinding(KqlTypeHelper.GetTypeDedfinition(KqlDataType.DateTimeType)));
            }

            if (detectionMode >= KqlTypeDetectionMode.StringsNumbersTimeAndComplexTypes)
            { 
                m_matchers.Add(new ColumnFinding(KqlTypeHelper.GetTypeDedfinition(KqlDataType.DynamicType)));
                m_matchers.Add(new ColumnFinding(KqlTypeHelper.GetTypeDedfinition(KqlDataType.GuidType)));
            }
    }

        public bool HasFindings => m_matchers.Any(x => x.MismatchCount == 0 && x.MatchCount > 0);
        
        public KqlTypeDefinition GetBestMatchColumnType()
        {
            var foundMatchers = m_matchers.Where(x => x.MismatchCount == 0 && x.MatchCount > 0).ToList();

            if (foundMatchers.Count > 0)
            {
                return foundMatchers[0].KqlTypeDef;
            }

            return s_stringDefinition;
        }

        public void AnalyzeField(string field)
        {
            if(string.IsNullOrEmpty(field))
            {
                return;
            }

            for (int i = 0; i < m_matchers.Count; i++)
            {
                if (m_matchers[i].KqlTypeDef.IsMatch(field))
                {
                    m_matchers[i].MatchCount++;
                }
                else
                {
                    m_matchers[i].MismatchCount++;
                }
            }
        }
    }
    #endregion

    #region Tabular Data Helper
    public static class TabularDataHelper
    {
        #region Public APIs
        public static bool TryDetectTabularTextFormat(string data, out char? separator)
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

        public static bool TryAnalyzeTabularData(string tableData, string delimiter, KqlTypeDetectionMode detectionMode, bool? isFirstRowHeader, out TableColumns scheme, out bool firstRowIsHeader)
        {
            using var stream = new MemoryStream(Encoding.UTF8.GetBytes(tableData));

            return TryAnalyzeTabularData(stream, delimiter, detectionMode, isFirstRowHeader, out scheme, out firstRowIsHeader);
        }

        public static bool TryAnalyzeTabularData(Stream inputStream, string delimiter, KqlTypeDetectionMode detectionMode, bool? isFirstRowHeader, out TableColumns scheme, out bool firstRowIsHeader)
        {
            var parser = new TextFieldParser(inputStream)
            {
                Delimiters = new string[] { delimiter },
                HasFieldsEnclosedInQuotes = true, 
            };

            return TryAnalyzeTabularData(parser, detectionMode, isFirstRowHeader, out scheme, out firstRowIsHeader);
        }

        public static bool TryConvertTableToInlineQuery(string tableData, string delimiter, KqlTypeDetectionMode detectionMode, bool? isFirstRowHeader, string? optionalKqlSuffix, out string inlineQuery)
        {
            inlineQuery = string.Empty;

            using var stream1 = new MemoryStream(Encoding.UTF8.GetBytes(tableData));
            
            if (!TryAnalyzeTabularData(stream1, delimiter, detectionMode, isFirstRowHeader, out var tableScheme, out var firstRowIsHeader))
            {
                return false;
            }

            using var stream2 = new MemoryStream(Encoding.UTF8.GetBytes(tableData));
            var queryBuilder = new StringBuilder();
            var parser = new TextFieldParser(stream2)
            {
                Delimiters = new string[] { delimiter },
                HasFieldsEnclosedInQuotes = true,
            };

            queryBuilder.AppendLine("// Query Created With Klipboard (https://github.com/yogilad/Klipboard/wiki)");
            queryBuilder.AppendLine("//");
            queryBuilder.Append("let Klipboard = datatable");
            queryBuilder.AppendLine(tableScheme.ToSchemaString(detectionMode));
            queryBuilder.AppendLine("[");

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
                    queryBuilder.Append(parsedField);
                    queryBuilder.Append(",");
                }

                queryBuilder.AppendLine("");
            }

            queryBuilder.AppendLine("];");
            queryBuilder.AppendLine("Klipboard");

            if (!string.IsNullOrWhiteSpace(optionalKqlSuffix))
            {
                queryBuilder.AppendLine(optionalKqlSuffix);
            }

            inlineQuery = queryBuilder.ToString();
            return true;
        }

        public static bool TryConvertFreeTextToInlineQuery(string freeText, string? optionalKqlSuffix, out string inlineQuery)
        {
            using var stream = new MemoryStream(Encoding.UTF8.GetBytes(freeText));
            var streamReader = new StreamReader(stream);
            var queryBuilder = new StringBuilder();

            queryBuilder.AppendLine("// Query Created With Klipboard (https://github.com/yogilad/Klipboard/wiki)");
            queryBuilder.AppendLine("//");
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

            if (!string.IsNullOrWhiteSpace(optionalKqlSuffix))
            {
                queryBuilder.AppendLine(optionalKqlSuffix);
            }

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
        private static bool TryAnalyzeTabularData(TextFieldParser parser, KqlTypeDetectionMode detectionMode, bool? isFirstRowHeader, out TableColumns scheme, out bool firstRowIsHeader)
        {
            scheme = new TableColumns();
            firstRowIsHeader = isFirstRowHeader ?? true;

            // Analyze First Row
            var firstRowfields = parser.ReadFields();

            if (firstRowfields == null || firstRowfields.Length == 0) 
            {
                return false;
            }

            var firstRowCols = new List<ColumnFindings>(firstRowfields.Length);

            for (int i = 0; i < firstRowfields.Length; i++)
            {
                firstRowCols.Add(new ColumnFindings(detectionMode));
                firstRowCols[i].AnalyzeField(firstRowfields[i]);
            }

            // If first row detection is required, determine if this is a header 
            if (isFirstRowHeader == null)
            {
                foreach (var col in firstRowCols)
                {
                    if (col.HasFindings)
                    {
                        firstRowIsHeader = false;
                        break;
                    }
                }
            }

            // Analazye remaining lines
            var rowNum = 0;

            var cols = new List<ColumnFindings>(firstRowfields.Length);
            while (rowNum < 100)
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

                for (int i = 0; i < fields.Length; i++)
                {
                    if (rowNum == 0)
                    {
                        cols.Add(new ColumnFindings(detectionMode));
                    }

                    cols[i].AnalyzeField(fields[i]);
                }

                rowNum++;
            }

            // If only header row, assume this is the data
            var colsToProcess = cols;
            if (rowNum == 0)
            {
                firstRowIsHeader = false;
                colsToProcess = firstRowCols;
            }

            // Combine detection results
            var colNo = 0;
            foreach(var col in colsToProcess)
            {
                scheme.Columns.Add((firstRowIsHeader ? firstRowfields[colNo] : $"Column_{colNo}", col.GetBestMatchColumnType()));
                colNo++;
            }
            return true;
        }
        #endregion
    }
    #endregion
}
