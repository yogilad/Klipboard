namespace Klipboard.Utils
{
    public static class KustoDatabaseHelperExtended
    {
        // Set format to null or AppConstants.UnknownFormat to trigger auto detection of textual data
        public static async Task<(bool Success, TableColumns? Schema, string? Format, string? Error)> TryAutoDetectTextBlobScheme(this KustoDatabaseHelper databaseHelper, string blobUri)
        {
            try
            {
                var csvSchemaRes = await databaseHelper.TryGetBlobSchemeAsync(blobUri, format: "csv");
                var tsvSchemaRes = await databaseHelper.TryGetBlobSchemeAsync(blobUri, format: "tsv");
                var multiJsonSchemaRes = await databaseHelper.TryGetBlobSchemeAsync(blobUri, format: "multijson");
                var jsonSchemaRes = await databaseHelper.TryGetBlobSchemeAsync(blobUri, format: "json");

                var curScheme = csvSchemaRes;
                if (tsvSchemaRes.Success && (!curScheme.Success || curScheme.TableScheme.Columns.Count < tsvSchemaRes.TableScheme.Columns.Count))
                {
                    curScheme = tsvSchemaRes;
                }

                if (multiJsonSchemaRes.Success && (!curScheme.Success || curScheme.TableScheme.Columns.Count < multiJsonSchemaRes.TableScheme.Columns.Count))
                {
                    curScheme = multiJsonSchemaRes;
                }

                if (jsonSchemaRes.Success && (!curScheme.Success || curScheme.TableScheme.Columns.Count < jsonSchemaRes.TableScheme.Columns.Count))
                {
                    curScheme = jsonSchemaRes;
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
