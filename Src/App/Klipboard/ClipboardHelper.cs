using System.Text;
using Kusto.Cloud.Platform.Utils;

using Klipboard.Utils;

namespace Klipboard
{
    public class ClipboardHelper : IClipboardHelper
    {
        public ClipboardContent GetClipboardContent()
        {
            // TODO: Consider supporting extracting html tables from HTML drop content 

            if (Clipboard.ContainsData(DataFormats.CommaSeparatedValue))
            {
                return ClipboardContent.CSV | ClipboardContent.CSV_Stream;
            }
            
            // Keep this last on the text data list as many types are also text represented (CSV, HTML, etc.)
            if (Clipboard.ContainsText())
            {
                return ClipboardContent.Text | ClipboardContent.Text_Stream;
            }

            if (Clipboard.ContainsFileDropList())
            {
                return ClipboardContent.Files;
            }

            return ClipboardContent.None;
        }

        public bool TryGetDataAsString(out string? data)
        {
            var content = GetClipboardContent() & (ClipboardContent.CSV | ClipboardContent.Text);

            if (content != ClipboardContent.None)
            {
                data = Clipboard.GetText();
                return true;
            }

            data = null;
            return false;
        }

        public bool TryGetDataAsMemoryStream(out Stream? stream)
        {
            var content = GetClipboardContent() & (ClipboardContent.CSV_Stream | ClipboardContent.Text_Stream);

            switch (content)
            {
                case ClipboardContent.CSV_Stream:
                    stream = Clipboard.GetData(DataFormats.CommaSeparatedValue) as MemoryStream;
                    break;

                case ClipboardContent.Text_Stream:
                    var data = Clipboard.GetText();
                    stream = new MemoryStream(Encoding.UTF8.GetBytes(data));
                    break;

                default:
                    stream = null;
                    break;
            }

            return stream != null;
        }

        public bool TryGetFileDropList(out List<string>? fileList)
        {
            fileList = null;

            if (GetClipboardContent() == ClipboardContent.Files)
            {
                var files = Clipboard.GetFileDropList();

                if (files != null)
                {
                    fileList = new List<string>();
                    foreach (var file in files)
                    {
                        if (file != null)
                        {
                            fileList.Add(file);
                        }
                    }
                }
            }

            return fileList.SafeFastAny();
        }
    }
}
