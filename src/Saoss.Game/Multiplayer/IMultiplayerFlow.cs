namespace Saoss.Game.Multiplayer;

/// <summary>A network game in progress on this machine, hosted or joined; updated once per frame before the screen.</summary>
public interface IMultiplayerFlow : IDisposable
{
    /// <summary>Handles network traffic and switches screens when the session changes state.</summary>
    void Update();
}
