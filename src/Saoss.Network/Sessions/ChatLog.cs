namespace Saoss.Network.Sessions;

/// <summary>The most recent chat lines of a session, oldest first.</summary>
internal sealed class ChatLog
{
    /// <summary>Lines kept; older ones are dropped.</summary>
    public const int Capacity = 100;

    private readonly List<ChatLine> _lines = [];

    /// <summary>The kept lines.</summary>
    public IReadOnlyList<ChatLine> Lines => _lines;

    /// <summary>Adds a line, dropping the oldest when full.</summary>
    public void Add(ChatLine line)
    {
        if (_lines.Count == Capacity)
        {
            _lines.RemoveAt(0);
        }

        _lines.Add(line);
    }
}
