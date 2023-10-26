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
            if (!Uri.TryCreate(clusterUri, UriKind.Absolute, out var uri))
            {
                error = "ClusterUri is not a valid Uri.";
                return false;
            }
            
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
                string uriParam = $"Data Source={clusterUri};Initial Catalog={databaseName}";
                queryLink = new Uri(uri, $"?uri={uriParam}&query={query}&web=0").ToString();
            }
            else
            {
                queryLink = new Uri(new Uri("https://dataexplorer.azure.com/"), $"/clusters/{uri.Host}/databases/{databaseName}?&query={query}").ToString();
            }

            OpSysHelper.InvokeLink(queryLink);
            return true;
        }
    }
}
