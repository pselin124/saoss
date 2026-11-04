using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Xna.Framework.Input;
using Saoss.Game.Input;

namespace Saoss.Game.Settings;

/// <summary>
/// Reads and writes <see cref="GameSettings"/> as JSON in a settings folder, normally
/// %LOCALAPPDATA%\Saoss\settings.json. Replaces the original's sets.txt.
/// </summary>
/// <param name="directory">Folder of the settings file; created on the first save.</param>
public sealed class SettingsStore(string directory)
{
    /// <summary>Name of the settings file.</summary>
    public const string FileName = "settings.json";

    private static readonly JsonSerializerOptions Options = new()
    {
        WriteIndented = true,
        Converters =
        {
            new LenientEnumConverter<Keys>(),
            new LenientEnumConverter<InputDevice>(),
            new JsonStringEnumConverter(),
        },
    };

    /// <summary>The per-user settings folder.</summary>
    public static string DefaultDirectory { get; } =
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Saoss");

    /// <summary>Full path of the settings file.</summary>
    public string FilePath { get; } = Path.Combine(directory, FileName);

    /// <summary>
    /// Loads the settings with every value clamped to its range. A missing, unreadable or corrupt file gives the
    /// defaults.
    /// </summary>
    public GameSettings Load()
    {
        try
        {
            if (!File.Exists(FilePath))
            {
                return new GameSettings();
            }

            var settings = JsonSerializer.Deserialize<GameSettings>(File.ReadAllText(FilePath), Options);
            return (settings ?? new GameSettings()).Validated();
        }
        catch (Exception e) when (e is JsonException or IOException or UnauthorizedAccessException)
        {
            return new GameSettings();
        }
    }

    /// <summary>
    /// Writes the settings. If the file cannot be written the settings still apply for this session, like the
    /// original when saving sets.txt failed.
    /// </summary>
    public void Save(GameSettings settings)
    {
        try
        {
            Directory.CreateDirectory(directory);
            File.WriteAllText(FilePath, JsonSerializer.Serialize(settings, Options));
        }
        catch (Exception e) when (e is IOException or UnauthorizedAccessException)
        {
        }
    }
}
