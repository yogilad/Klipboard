using Kusto.Data;


namespace Klipboard.Utils
{
    public static class InlineQueryHelper
    {
        public static bool TryInvokeInlineQuery(AppConfig appConfig, string actionName, KustoConnectionStringBuilder clusterKcsb, string databaseName, string query, INotificationHelper notificationHelper, out string? error)
        {
            return TryInvokeInlineQuery(appConfig, actionName, clusterKcsb.DataSource, databaseName, query, notificationHelper, out error);
        }

        public static bool TryInvokeInlineQuery(AppConfig appConfig, string actionName, string clusterUri, string databaseName, string query, INotificationHelper notificationHelper, out string? error)
        {
            string queryLink;
            string uriParam;

            error = null;

            if (string.IsNullOrWhiteSpace(query))
            {
                error = "Query string is empty.";
                return false;
            }

            if (appConfig.DefaultQueryApp == QueryApp.Clipboard)
            {
                notificationHelper.CopyResultNotification(actionName, "Query Is Ready", query);
                return true;
            }

            if (!Uri.TryCreate(clusterUri, UriKind.Absolute, out var uri))
            {
                error = "ClusterUri is not a valid Uri.";
                return false;
            }

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
