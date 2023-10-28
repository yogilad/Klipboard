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
            string uriParam;

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

            switch (appConfig.DefaultQueryApp)
            {
                case QueryApp.Desktop:
                    uriParam = $"Data Source={clusterUri};Initial Catalog={databaseName}";
                    queryLink = $"kusto://query?uri={Uri.EscapeDataString(uriParam)}&query={Uri.EscapeDataString(query)}";
                    break;

                case QueryApp.Web:
                default: // compilation fix
                    queryLink = $"https://dataexplorer.azure.com/clusters/{Uri.EscapeDataString(uri.Host)}/databases/{Uri.EscapeDataString(databaseName)}?&query={Uri.EscapeDataString(query)}";
                    break;
            }

            OpSysHelper.InvokeLink(queryLink);
            return true;
        }
    }
}
