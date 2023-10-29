using System.Text;

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

        public string? TryGetDataAsString()
        {
            var content = GetClipboardContent() & (ClipboardContent.CSV | ClipboardContent.Text);

            return content != ClipboardContent.None ? Clipboard.GetText() : null;

        }

        public Stream? TryGetDataAsMemoryStream()
        {
            var content = (GetClipboardContent()) & (ClipboardContent.CSV_Stream | ClipboardContent.Text_Stream);

            return content switch
            {
                ClipboardContent.CSV_Stream => Clipboard.GetData(DataFormats.CommaSeparatedValue) as MemoryStream,
                ClipboardContent.Text_Stream => new MemoryStream(Encoding.UTF8.GetBytes(Clipboard.GetText())),
                _ => null,
            };
        }

        public List<string>? TryGetFileDropList()
        {
            if ((GetClipboardContent()) != ClipboardContent.Files)
            {
                return null;
            }

            return Clipboard.GetFileDropList().Cast<string?>().Where(x => x != null).ToList()!;
        }

        public Task SetText(string text)
        {
            return StartSTATask(() => Clipboard.SetText(text));
        }

        private static Task StartSTATask(Action func)
        {
            var tcs = new TaskCompletionSource();

            Thread thread = new Thread(() =>
            {
                try
                {
                    func();
                    tcs.SetResult();
                }
                catch (Exception e)
                {
                    tcs.SetException(e);
                }
            });

            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
            return tcs.Task;
        }
    }
}
