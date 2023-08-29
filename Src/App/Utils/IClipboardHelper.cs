﻿namespace Klipboard.Utils
{
    public enum ClipboardContent
    {
        None,
        CSV,
        Text,
        Files
    }

    public interface IClipboardHelper
    {
        ClipboardContent GetClipboardContent();

        bool TryGetDataAsString(out string? data);

        bool TryGetDataAsMemoryStream(out Stream? stream);

        bool TryGetFileDropList(out List<string>? fileList);
    }
}