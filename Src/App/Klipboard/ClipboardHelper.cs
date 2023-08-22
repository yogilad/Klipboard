using Kusto.Cloud.Platform.Utils;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Klipboard
{
    public static class ClipboardHelper
    {
        public enum Content
        {
            None,
            CSV,
            Text,
            DropFiles
        }

        public static Content GetClipboardContent()
        {
            // TODO: Consider supporting extracting html tables from HTML drop content 

            if (Clipboard.ContainsData(DataFormats.CommaSeparatedValue))
            {
                return Content.CSV;
            }
            
            // Keep this last on the text data list as many types are also text represented (CSV, HTML, etc.)
            if (Clipboard.ContainsText())
            {
                return Content.Text;
            }

            if (Clipboard.ContainsFileDropList())
            {
                return Content.DropFiles;
            }

            return Content.None;
        }

        public static bool TryGetDataAsString(out string? data)
        {
            var content = GetClipboardContent();

            switch (content)
            {
                case Content.CSV:
                case Content.Text:
                    data = Clipboard.GetText();
                    break;

                default:
                    data = null;
                    break;
            }

            return data != null;
        }

        public static bool TryGetDataAsMemoryStream(out Stream? stream)
        {
            var content = GetClipboardContent();

            switch (content)
            {
                case Content.CSV:
                    stream = Clipboard.GetData(DataFormats.CommaSeparatedValue) as MemoryStream;
                    break;

                case Content.Text:
                    var data = Clipboard.GetText();
                    stream = new MemoryStream(Encoding.UTF8.GetBytes(data));
                    break;

                default:
                    stream = null;
                    break;
            }

            return stream != null;
        }

        public static bool TryGetFileDropList(out List<string>? fileList)
        {
            fileList = null;

            if (GetClipboardContent() == Content.DropFiles)
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
