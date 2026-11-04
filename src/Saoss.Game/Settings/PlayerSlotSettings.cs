namespace Saoss.Game.Settings;

/// <summary>
/// Settings of one player slot [sets->ihminen, sets->team].
/// </summary>
/// <param name="IsHuman">Played from the keyboard rather than by the AI.</param>
/// <param name="Team">Team number, 0 to <see cref="GameSettings.MaxTeam"/> (shown one higher in the menu).</param>
public sealed record PlayerSlotSettings(bool IsHuman, int Team);
