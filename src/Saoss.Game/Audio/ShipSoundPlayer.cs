using Microsoft.Xna.Framework.Audio;
using Saoss.Core.Simulation;

namespace Saoss.Game.Audio;

/// <summary>
/// Plays the ship sounds a match's events ask for. One-shot sounds are fire-and-forget; a looping sound (lasers,
/// the Peace Cannon beam) is tracked per ship so <see cref="ShipLoopStopped"/> can stop it, like the original's
/// per-player Sound copy whose stop() stopped its last channel.
/// </summary>
public sealed class ShipSoundPlayer : IDisposable
{
    // The original's FMOD mixer had 64 channels; older restarted loop sounds beyond this are cut.
    private const int MaxReleasedInstances = 32;

    private readonly SoundBank _bank;
    private readonly Dictionary<int, SoundEffectInstance> _loops = [];
    private readonly List<SoundEffectInstance> _released = [];
    private bool _enabled = true;

    /// <summary>Creates a player for the sounds of <paramref name="bank"/>.</summary>
    public ShipSoundPlayer(SoundBank bank) => _bank = bank;

    /// <summary>Sound effects on [efektitpaalla]; turning them off stops every sound this player started.</summary>
    public bool Enabled
    {
        get => _enabled;
        set
        {
            _enabled = value;
            if (!value)
            {
                StopAll();
            }
        }
    }

    /// <summary>Sound volume setting, 0-100 [soundvolume]; applies to sounds started afterwards.</summary>
    public int Volume { get; set; } = 100;

    /// <summary>Plays the sound events among <paramref name="events"/>.</summary>
    public void Play(IReadOnlyList<GameEvent> events)
    {
        if (!_enabled)
        {
            return;
        }

        foreach (var gameEvent in events)
        {
            switch (gameEvent)
            {
                case ShipSoundPlayed played:
                    Play(played);
                    break;
                case ShipLoopStopped stopped:
                    StopLoop(stopped.ShipIndex);
                    break;
            }
        }
    }

    /// <summary>Frees the instances of replaced loop sounds that have finished.</summary>
    public void Update() => _released.RemoveAll(DisposeIfStopped);

    /// <summary>Stops every looping sound, e.g. when the match is left or paused.</summary>
    public void StopAll()
    {
        foreach (var instance in _loops.Values.Concat(_released))
        {
            StopAndDispose(instance);
        }

        _loops.Clear();
        _released.Clear();
    }

    /// <inheritdoc/>
    public void Dispose() => StopAll();

    private static bool DisposeIfStopped(SoundEffectInstance instance)
    {
        if (instance.State != SoundState.Stopped)
        {
            return false;
        }

        instance.Dispose();
        return true;
    }

    private static void StopAndDispose(SoundEffectInstance instance)
    {
        instance.Stop();
        instance.Dispose();
    }

    private void Play(ShipSoundPlayed played)
    {
        if (!_bank.TryGet(played.SoundOf, played.Sound, out var effect, out var spec))
        {
            return;
        }

        var volume = AudioVolume.Effect(spec.Volume, Volume);
        if (!played.Loop)
        {
            if (volume > 0f)
            {
                effect.Play(volume, 0f, 0f);
            }

            return;
        }

        ReleaseLoop(played.ShipIndex);
        if (volume <= 0f)
        {
            return;
        }

        var instance = effect.CreateInstance();
        instance.IsLooped = spec.Loops;
        instance.Volume = volume;
        try
        {
            instance.Play();
            _loops[played.ShipIndex] = instance;
        }
        catch (InstancePlayLimitException)
        {
            instance.Dispose();
        }
    }

    // A restarted non-looping sound keeps playing to its end, as FMOD played every play() on a free channel.
    private void ReleaseLoop(int shipIndex)
    {
        if (!_loops.Remove(shipIndex, out var previous))
        {
            return;
        }

        if (previous.IsLooped)
        {
            StopAndDispose(previous);
            return;
        }

        if (_released.Count >= MaxReleasedInstances)
        {
            StopAndDispose(_released[0]);
            _released.RemoveAt(0);
        }

        _released.Add(previous);
    }

    private void StopLoop(int shipIndex)
    {
        if (_loops.Remove(shipIndex, out var instance))
        {
            StopAndDispose(instance);
        }
    }
}
