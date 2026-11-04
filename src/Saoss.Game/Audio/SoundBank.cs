using System.Diagnostics.CodeAnalysis;
using Microsoft.Xna.Framework.Audio;
using Saoss.Core.Ships;
using Saoss.Core.Simulation;

namespace Saoss.Game.Audio;

/// <summary>
/// The ship sound effects, loaded once from Content/Sounds and looked up through <see cref="ShipSoundTable"/>.
/// Ships that share a file share one <see cref="SoundEffect"/>, like the original's sound copies shared a sample.
/// </summary>
public sealed class SoundBank : IDisposable
{
    private readonly Dictionary<string, SoundEffect> _effects;

    private SoundBank(Dictionary<string, SoundEffect> effects) => _effects = effects;

    /// <summary>
    /// Loads every file of <see cref="ShipSoundTable.Files"/> from <paramref name="folder"/>. Throws
    /// <see cref="NoAudioHardwareException"/> when there is no audio device.
    /// </summary>
    public static SoundBank Load(string folder)
    {
        var effects = new Dictionary<string, SoundEffect>(StringComparer.OrdinalIgnoreCase);
        try
        {
            foreach (var file in ShipSoundTable.Files)
            {
                effects[file] = SoundEffect.FromFile(Path.Combine(folder, file));
            }

            return new SoundBank(effects);
        }
        catch
        {
            foreach (var effect in effects.Values)
            {
                effect.Dispose();
            }

            throw;
        }
    }

    /// <summary>
    /// Finds the sound <paramref name="sound"/> of <paramref name="ship"/>; false when the original had none.
    /// </summary>
    public bool TryGet(ShipType ship, ShipSound sound, [NotNullWhen(true)] out SoundEffect? effect, [NotNullWhen(true)] out ShipSoundSpec? spec)
    {
        spec = ShipSoundTable.Get(ship, sound);
        effect = spec == null ? null : _effects.GetValueOrDefault(spec.File);
        return effect != null;
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        foreach (var effect in _effects.Values)
        {
            effect.Dispose();
        }

        _effects.Clear();
    }
}
