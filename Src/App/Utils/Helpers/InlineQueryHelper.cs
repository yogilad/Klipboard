using System.Diagnostics;

using Kusto.Data;


namespace Klipboard.Utils
{
    public static class InlineQueryHelper
    {
        public static bool TryInvokeInlineQuery(AppConfig appConfig, KustoConnectionStringBuilder clusterKcsb, string databaseName, string query, out string? error)
        {
            return TryInvokeInlineQuery(appConfig, clusterKcsb.DataSource, databaseName, query, out error);
        }

        public static bool TryInvokeInlineQuery(AppConfig appConfig, string clusterUri, string databaseName, string query, out string? error)
        {
            var clusterHost = clusterUri.Trim().TrimEnd('/').Replace("https://", "");
            string queryLink;

            if (string.IsNullOrWhiteSpace(query))
            {
                error = "Query string is empty.";
                return false;
            }

            error = null;
            query = TabularDataHelper.GzipBase64(query);

            if (AppConstants.EnforceInlineQuerySizeLimits && query.Length > AppConstants.MaxAllowedQueryLength)
            {
                error = $"Resulting query link excceds {AppConstants.MaxAllowedQueryLengthKB}KB.";
                return false;
            }

            if (appConfig.DefaultQueryApp == QueryApp.Desktop)
            {
                queryLink = $"https://{clusterHost}/{databaseName}?query={query}&web=0";
            }
            else
            {
                queryLink = $"https://dataexplorer.azure.com/clusters/{clusterHost}/databases/{databaseName}?query={query}";
            }
            Debug.WriteLine(queryLink);

            Process.Start(new ProcessStartInfo
            {
                FileName = queryLink,
                UseShellExecute = true
            });

            return true;
        }
    }
}
