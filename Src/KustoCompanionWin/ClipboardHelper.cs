using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace KustoCompanionWin
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

        public static object? GetDataAsString(int linelimit = -1)
        {
            var content = GetClipboardContent();

            switch (content)
            {
                case Content.CSV:
                    var csv = Clipboard.GetData(DataFormats.CommaSeparatedValue);
                    return csv;

                case Content.Text:
                    var text = Clipboard.GetText();
                    return text;

                case Content.DropFiles:
                    var files = Clipboard.GetFileDropList();
                    return files;

                default:
                    return null;
            }
        }

        public static object? GetDataAsIstream()
        {
            var content = GetClipboardContent();

            switch (content)
            {
                case Content.CSV:
                    var csv = Clipboard.GetData(DataFormats.CommaSeparatedValue);
                    return csv;

                case Content.Text:
                    var text = Clipboard.GetText();
                    return text;

                case Content.DropFiles:
                    var files = Clipboard.GetFileDropList();
                    return files;

                default:
                    return null;
            }
        }
    }
}
