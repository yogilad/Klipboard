using System.Text;

using Klipboard.Utils;


namespace Klipboard
{
    public class ClipboardHelper : IClipboardHelper
    {
        public Task<ClipboardContent> GetClipboardContent()
        {
            // TODO: Consider supporting extracting html tables from HTML drop content

            if (Clipboard.ContainsData(DataFormats.CommaSeparatedValue))
            {
                return Task.FromResult(ClipboardContent.CSV | ClipboardContent.CSV_Stream);
            }

            // Keep this last on the text data list as many types are also text represented (CSV, HTML, etc.)
            if (Clipboard.ContainsText())
            {
                return Task.FromResult(ClipboardContent.Text | ClipboardContent.Text_Stream);
            }

            if (Clipboard.ContainsFileDropList())
            {
                return Task.FromResult(ClipboardContent.Files);
            }

            return Task.FromResult(ClipboardContent.None);
        }

        public async Task<string?> TryGetDataAsString()
        {
            var content = await GetClipboardContent() & (ClipboardContent.CSV | ClipboardContent.Text);

            return content != ClipboardContent.None ? Clipboard.GetText() : null;

        }

        public async Task<Stream?> TryGetDataAsMemoryStream()
        {
            var content = (await GetClipboardContent()) & (ClipboardContent.CSV_Stream | ClipboardContent.Text_Stream);

            return content switch
            {
                ClipboardContent.CSV_Stream => Clipboard.GetData(DataFormats.CommaSeparatedValue) as MemoryStream,
                ClipboardContent.Text_Stream => new MemoryStream(Encoding.UTF8.GetBytes(Clipboard.GetText())),
                _ => null,
            };
        }

        public async Task<List<string>?> TryGetFileDropList()
        {
            if ((await GetClipboardContent()) != ClipboardContent.Files)
            {
                return null;
            }

            return Clipboard.GetFileDropList().Cast<string?>().Where(x => x != null).ToList()!;
        }
    }
}
