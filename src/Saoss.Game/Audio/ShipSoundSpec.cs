namespace Saoss.Game.Audio;

/// <summary>
/// One entry of a ship type's sound set: which converted sound file it plays and how loud.
/// </summary>
/// <param name="File">File name in Content/Sounds.</param>
/// <param name="Volume">
/// Volume relative to the sound volume setting, from the original's setSoundVolumes; 0 means the original loaded the
/// sound but muted it.
/// </param>
/// <param name="Loops">Whether the sample itself loops (the original loaded it with FSOUND_LOOP_NORMAL).</param>
public sealed record ShipSoundSpec(string File, float Volume, bool Loops = false);
