using System.Globalization;
using System.Runtime.InteropServices;
using System.Text;

namespace Saoss.Game.Diagnostics;

/// <summary>
/// Last-resort handling of unhandled exceptions: the exception is appended to crash.log in the settings folder and a
/// message box tells the player where it is. Nothing is sent anywhere.
/// </summary>
public static class CrashReporter
{
    /// <summary>Name of the crash log file.</summary>
    public const string FileName = "crash.log";

    private const uint ErrorIcon = 0x10;

    /// <summary>Handles unhandled exceptions of every thread from now on, logging to <paramref name="directory"/>.</summary>
    public static void Install(string directory) =>
        AppDomain.CurrentDomain.UnhandledException += (_, e) => Report(directory, e.ExceptionObject);

    /// <summary>The crash log entry for <paramref name="exception"/>.</summary>
    public static string FormatEntry(object exception, DateTime time)
    {
        var entry = new StringBuilder();
        entry.Append(CultureInfo.InvariantCulture, $"==== {time:yyyy-MM-dd HH:mm:ss} Saoss {typeof(CrashReporter).Assembly.GetName().Version}");
        entry.AppendLine(CultureInfo.InvariantCulture, $" on {Environment.OSVersion}, .NET {Environment.Version}");
        entry.AppendLine(exception.ToString());
        return entry.ToString();
    }

    private static void Report(string directory, object exception)
    {
        var path = Path.Combine(directory, FileName);
        try
        {
            Directory.CreateDirectory(directory);
            File.AppendAllText(path, FormatEntry(exception, DateTime.Now));
        }
        catch (Exception e) when (e is IOException or UnauthorizedAccessException)
        {
            path = "(the log could not be written)";
        }

        if (!OperatingSystem.IsWindows())
        {
            Console.Error.WriteLine($"Saoss crashed; details were saved to {path}");
            return;
        }

        var message = exception is Exception error ? error.Message : exception.ToString();
        _ = MessageBoxW(IntPtr.Zero, $"Saoss crashed: {message}\n\nDetails were saved to:\n{path}", "Saoss", ErrorIcon);
    }

    [DllImport("user32.dll", CharSet = CharSet.Unicode)]
    private static extern int MessageBoxW(IntPtr owner, string text, string caption, uint type);
}
