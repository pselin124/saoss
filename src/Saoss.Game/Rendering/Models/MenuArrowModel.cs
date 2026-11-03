namespace Saoss.Game.Rendering.Models;

/// <summary>
/// The silver arrow that points at the selected menu item (original <c>NUOLI</c> display list).
/// It points along +X.
/// </summary>
public static class MenuArrowModel
{
    /// <summary>Emits the arrow: an elongated ellipsoid shaft and a three-sided head.</summary>
    public static void Build(MeshBuilder builder)
    {
        builder.PushMatrix();
        builder.SetColor(0.8f, 0.8f, 0.8f);
        builder.Scale(2f, 0.4f, 0.2f);
        builder.Sphere(1f, 15, 10);
        builder.Scale(1f / 2f, 2.5f, 1.5f);
        builder.Translate(2f, 0f, 0f);
        builder.Rotate(30f, 0f, 0f, 1f);
        builder.Sphere(1f, 3, 15);
        builder.PopMatrix();
    }
}
