using System.Runtime.InteropServices;

namespace Saoss.Game.Input;

/// <summary>Reads text from the Windows clipboard (the original's chat and readnick paste).</summary>
public static class ClipboardText
{
    private const uint UnicodeTextFormat = 13;

    /// <summary>The clipboard's text, or null when it holds no text or cannot be opened.</summary>
    public static string? Read()
    {
        if (!IsClipboardFormatAvailable(UnicodeTextFormat) || !OpenClipboard(IntPtr.Zero))
        {
            return null;
        }

        try
        {
            var handle = GetClipboardData(UnicodeTextFormat);
            var text = handle == IntPtr.Zero ? IntPtr.Zero : GlobalLock(handle);
            if (text == IntPtr.Zero)
            {
                return null;
            }

            try
            {
                return Marshal.PtrToStringUni(text);
            }
            finally
            {
                GlobalUnlock(handle);
            }
        }
        finally
        {
            CloseClipboard();
        }
    }

    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool IsClipboardFormatAvailable(uint format);

    [DllImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool OpenClipboard(IntPtr owner);

    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool CloseClipboard();

    [DllImport("user32.dll")]
    private static extern IntPtr GetClipboardData(uint format);

    [DllImport("kernel32.dll")]
    private static extern IntPtr GlobalLock(IntPtr memory);

    [DllImport("kernel32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool GlobalUnlock(IntPtr memory);
}
