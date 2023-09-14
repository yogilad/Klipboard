namespace Klipboard.Utils
{
    [Flags]
    public enum  ClipboardContent
    {
        None        = 0,
        CSV         = 1,
        CSV_Stream  = 2,
        Text        = 4,
        Text_Stream = 8,
        Files       = 16,
    }

    public interface IClipboardHelper
    {
        ClipboardContent GetClipboardContent();
        string? TryGetDataAsString();
        Stream? TryGetDataAsMemoryStream();
        List<string>? TryGetFileDropList();
    }
}
