namespace Klipboard.Utils
{
    [Flags]
    public enum  ClipboardContent
    {
        None,
        CSV,
        CSV_Stream,
        Text,
        Text_Stream,
        Files,
    }

    public interface IClipboardHelper
    {
        ClipboardContent GetClipboardContent();

        bool TryGetDataAsString(out string? data);

        bool TryGetDataAsMemoryStream(out Stream? stream);

        bool TryGetFileDropList(out List<string>? fileList);
    }
}
