namespace Saoss.Game.Rendering.Legacy;

/// <summary>What a <see cref="GlListCommand"/> does when the list is replayed.</summary>
internal enum GlListOp
{
    /// <summary>Calls <see cref="GlListCommand.Call"/>.</summary>
    Call,

    /// <summary>glVertex3f with X, Y, Z.</summary>
    Vertex,

    /// <summary>glNormal3f with X, Y, Z.</summary>
    Normal,

    /// <summary>glTexCoord2f with X, Y.</summary>
    TexCoord,

    /// <summary>glColor4f with X, Y, Z, W.</summary>
    Color,

    /// <summary>glBegin with the <see cref="GlPrimitive"/> in X.</summary>
    Begin,

    /// <summary>glEnd.</summary>
    End,
}
