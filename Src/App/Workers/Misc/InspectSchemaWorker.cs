using System.Text;
using System.Text.RegularExpressions;

using Klipboard.Utils;

namespace Klipboard.Workers
{
    public class InspectSchemaWorker : WorkerBase
    {     
        private const string NotificationTitle = "Inspect Schema";
        private const string FirstRowIsHeader = "First Row Is Header";
        private const string NoHeaderRow = "No Header Row";


        public InspectSchemaWorker(ISettings settings, INotificationHelper notificationHelper)
            : base(ClipboardContent.Files | ClipboardContent.CSV | ClipboardContent.Text, settings, notificationHelper, new List<string>() { FirstRowIsHeader, NoHeaderRow })
        {
        }

        public override string GetMenuText(ClipboardContent content) => "Inspect Schema";

        public override string GetToolTipText() => "Inspect the schema of tabular data or up to 100 files";

        public override bool IsMenuVisible() => AppConstants.DevMode;

        public override async Task HandleCsvAsync(string csvData, string? chosenOption)
        {
            var progressNotification = m_notificationHelper.ShowProgressNotification(NotificationTitle, $"Clipboard Table", "Preparing Data", "");

            progressNotification.UpdateProgress("", 0, "0/1");

            var upstreamFileName = FileHelper.CreateUploadFileName("Table", "tsv");
            var target = GetQuickActionTarget();
            using var databaseHelper = new KustoDatabaseHelper(target.ConnectionString, target.DatabaseName);
            using var csvStream = new MemoryStream(Encoding.UTF8.GetBytes(csvData));
            var firstRowIsHeader = FirstRowIsHeader.Equals(chosenOption);
            var result = await HandleSingleTextStreamAsync(databaseHelper, csvStream, FileHelper.TsvFormatDefinition, upstreamFileName, firstRowIsHeader);
            
            if (result.Success)
            {
                progressNotification.UpdateProgress("Done", 1, "1/1");
                progressNotification.CloseNotification(withinSeconds: 5);
                m_notificationHelper.ShowExtendedNotification(NotificationTitle, "Clipboard Table Schema Result", $"Schema: {result.Schema.ToSchemaString()}\r\nFormat: {result.Format}");
            }
            else
            {
                progressNotification.UpdateProgress("Failed", 1, "1/1");
                progressNotification.CloseNotification(withinSeconds: 5);
                m_notificationHelper.ShowExtendedNotification(NotificationTitle, "Clipboard Table Schema Detection Failed", result.Error);
            }
        }

        public override async Task HandleTextAsync(string textData, string? chosenOption)
        {
            var progressNotification = m_notificationHelper.ShowProgressNotification(NotificationTitle, $"Clipboard Text", "Preparing Data", "");

            progressNotification.UpdateProgress("", 0, "0/1");

            var upstreamFileName = FileHelper.CreateUploadFileName("Text", "txt");
            var target = GetQuickActionTarget();
            using var databaseHelper = new KustoDatabaseHelper(target.ConnectionString, target.DatabaseName);
            using var textStream = new MemoryStream(Encoding.UTF8.GetBytes(textData));
            var firstRowIsHeader = FirstRowIsHeader.Equals(chosenOption);
            var result = await HandleSingleTextStreamAsync(databaseHelper, textStream, FileHelper.UnknownFormatDefinition, upstreamFileName, firstRowIsHeader);

            if (result.Success)
            {
                progressNotification.UpdateProgress("Done", 1, "1/1");
                progressNotification.CloseNotification(withinSeconds: 5);
                m_notificationHelper.ShowExtendedNotification(NotificationTitle, "Clipboard Text", $"Schema: {result.Schema.ToSchemaString()}\r\nFormat: {result.Format}");
            }
            else
            {
                progressNotification.UpdateProgress("Failed", 1, "1/1");
                progressNotification.CloseNotification(withinSeconds: 5);
                m_notificationHelper.ShowExtendedNotification(NotificationTitle, "Clipboard Text Schema Detection Failed", result.Error);
            }
        }

        public override async Task HandleFilesAsync(List<string> filesAndFolders, string? chosenOption)
        {
            var progressNotification = m_notificationHelper.ShowProgressNotification(NotificationTitle, $"Clipboard Files", "Preparing Data", "");
            var fileList = new List<string>();
            var firstRowIsHeader = FirstRowIsHeader.Equals(chosenOption);
            var target = GetQuickActionTarget();
            var curFileNo = 0.0;
            using var databaseHelper = new KustoDatabaseHelper(target.ConnectionString, target.DatabaseName);
            var report = new StringBuilder();

            progressNotification.UpdateProgress("Listing Files", 0, $"0/{fileList.Count}");

            foreach (var path in FileHelper.ExpandDropFileList(filesAndFolders))
            {
                if (fileList.Count == 100)
                {
                    break;
                }

                fileList.Add(path);
            }

            progressNotification.UpdateProgress("Listing Files", 0, $"0/{fileList.Count}");
            report.AppendLine("No\tFile\tFormat\tSchema\tError");

            foreach (var filePath in fileList) 
            {
                var fileInfo = new FileInfo(filePath);
                var formatResult = FileHelper.GetFormatFromFileName(fileInfo.Name);
                var upstreamFileName = FileHelper.CreateUploadFileName(fileInfo.Name);
                using var fileStream = File.OpenRead(filePath);
                var result = await HandleSingleTextStreamAsync(databaseHelper, fileStream, formatResult, upstreamFileName, firstRowIsHeader, filePath);
                var escapedSchema = string.Empty;
                var escapedError = string.Empty;

                if (result.Schema != null)
                {
                    escapedSchema = Regex.Replace(result.Schema.ToString(), @"\p{Cc}", a => string.Format("[{0:X2}]", (byte)a.Value[0]));
                }

                if (result.Error != null)
                {
                    escapedError = Regex.Replace(result.Error, @"\p{Cc}", a => string.Format("[{0:X2}]", (byte)a.Value[0]));
                }

                curFileNo++;
                report.Append(curFileNo);
                report.Append("\t");
                report.Append(filePath);
                report.Append("\t");
                report.Append(result.Format);
                report.Append("\t");
                report.Append(escapedSchema);
                report.Append("\t");
                report.AppendLine(escapedError);

                progressNotification.UpdateProgress("Inspecting Files", curFileNo / fileList.Count, $"{curFileNo}/{fileList.Count}");
            }

            progressNotification.UpdateProgress("Done", 1.0, $"{curFileNo}/{fileList.Count}");
            progressNotification.CloseNotification(5);
            m_notificationHelper.ShowExtendedNotification(NotificationTitle, $"Clipboard Files ({curFileNo})", report.ToString());
        }

        private async Task<(bool Success, TableColumns? Schema, string? Format, string? Error)> HandleSingleTextStreamAsync(KustoDatabaseHelper databaseHelper,
            Stream dataStream,
            FileFormatDefiniton formatDefinition,
            string upstreamFileName,
            bool firstRowIsHeader,
            string? filePath = null)
        {
            var uploadRes = await databaseHelper.TryUploadFileToEngineStagingAreaAsync(dataStream, upstreamFileName, formatDefinition, filePath);

            if (!uploadRes.Success)
            {
                return (false, null, null, $"Failed to upload file: {uploadRes.Error}"); ;
            }


            string schemaStr = AppConstants.TextLinesSchemaStr;

            if (formatDefinition.Extension == AppConstants.UnknownFormat)
            {
                return await databaseHelper.TryAutoDetectTextBlobScheme(uploadRes.BlobUri, firstRowIsHeader);
            }
            else
            {
                return await databaseHelper.TryGetBlobSchemeAsync(uploadRes.BlobUri, formatDefinition.Extension, firstRowIsHeader);
            }
        }

        private Cluster GetQuickActionTarget()
        {
            return m_settings.GetConfig().ChosenCluster;
        }
    }
}
