﻿using Kusto.Cloud.Platform.Utils;

namespace Klipboard.Utils
{
    public static class KustoDatabaseHelperExtended
    {
        // Set format to null or AppConstants.UnknownFormat to trigger auto detection of textual data
        public static async Task<(bool Success, TableColumns? Schema, string? Format, string? Error)> TryAutoDetectTextBlobScheme(this KustoDatabaseHelper databaseHelper, string blobUri, bool firstRowIsHeader = false)
        {
            try
            {
                var csvSchemaRes = await databaseHelper.TryGetBlobSchemeAsync(blobUri, format: "csv", firstRowIsHeader);
                var tsvSchemaRes = await databaseHelper.TryGetBlobSchemeAsync(blobUri, format: "tsv", firstRowIsHeader);
                var multiJsonSchemaRes = await databaseHelper.TryGetBlobSchemeAsync(blobUri, format: "multijson", firstRowIsHeader);
                var jsonSchemaRes = await databaseHelper.TryGetBlobSchemeAsync(blobUri, format: "json", firstRowIsHeader);
                
                var curScheme = multiJsonSchemaRes;

                if (jsonSchemaRes.Success && (!curScheme.Success || curScheme.TableScheme.Columns.Count < jsonSchemaRes.TableScheme.Columns.Count))
                {
                    curScheme = jsonSchemaRes;
                }

                if (tsvSchemaRes.Success && 
                    tsvSchemaRes.TableScheme.Columns.Count > 1 && 
                    (!curScheme.Success || curScheme.TableScheme.Columns.Count < tsvSchemaRes.TableScheme.Columns.Count))
                {
                    curScheme = tsvSchemaRes;
                }

                if (csvSchemaRes.Success &&
                    csvSchemaRes.TableScheme.Columns.Count > 1 &&
                    (!curScheme.Success || curScheme.TableScheme.Columns.Count < csvSchemaRes.TableScheme.Columns.Count))
                {
                    curScheme = csvSchemaRes;
                }

                if (curScheme.Success)
                {
                    return (true, curScheme.TableScheme, curScheme.format, null);
                }
                
                return (true, AppConstants.TextLinesSchema, "txt", null);
            }
            catch (Exception ex)
            {
                return (false, null, null, $"Failed to auto detect text scheme: {ex.Message}");
            }
        }
    }
}
