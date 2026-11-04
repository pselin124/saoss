namespace Saoss.Game.Screens.Network;

/// <summary>
/// The arrow of a lobby screen (net.cpp hostMenu and joinwaitforstart): column 0 is the menu, the other columns
/// are columns of the slot table. Left and Right move between columns with wrap-around, Up and Down within the
/// column with wrap-around. The row is kept in range when the column or the table size changes.
/// </summary>
/// <param name="menuItems">Entries of the menu column.</param>
/// <param name="tableColumns">Selectable columns of the table.</param>
public sealed class LobbyCursor(int menuItems, int tableColumns)
{
    /// <summary>0 for the menu, 1 to the table column count for the table.</summary>
    public int Column { get; private set; }

    /// <summary>Menu entry or table row.</summary>
    public int Row { get; private set; }

    /// <summary>Whether the arrow is in the menu column.</summary>
    public bool InMenu => Column == 0;

    /// <summary>Moves one column left.</summary>
    public void Left(int tableRows)
    {
        Column = Column > 0 ? Column - 1 : tableColumns;
        Clamp(tableRows);
    }

    /// <summary>Moves one column right.</summary>
    public void Right(int tableRows)
    {
        Column = Column < tableColumns ? Column + 1 : 0;
        Clamp(tableRows);
    }

    /// <summary>Moves one row up.</summary>
    public void Up(int tableRows)
    {
        var last = LastRow(tableRows);
        Row = Row > 0 ? Math.Min(Row - 1, last) : last;
    }

    /// <summary>Moves one row down.</summary>
    public void Down(int tableRows) => Row = Row < LastRow(tableRows) ? Row + 1 : 0;

    /// <summary>Puts the arrow on the last menu entry (Escape).</summary>
    public void SelectLastMenuItem()
    {
        Column = 0;
        Row = menuItems - 1;
    }

    /// <summary>Keeps the row inside the column, e.g. after the table shrank.</summary>
    public void Clamp(int tableRows) => Row = Math.Clamp(Row, 0, LastRow(tableRows));

    private int LastRow(int tableRows) => (InMenu ? menuItems : tableRows) - 1;
}
