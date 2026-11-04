namespace Saoss.Game.Rendering.Legacy;

/// <summary>
/// The stencil test settings of glStencilFunc and glStencilOp. The test passes when
/// <c>(Reference &amp; Mask) Function (stencil &amp; Mask)</c> holds.
/// </summary>
/// <param name="Function">Comparison of the masked reference against the masked stored value.</param>
/// <param name="Reference">Reference value, also what <see cref="GlStencilOp.Replace"/> writes.</param>
/// <param name="Mask">Mask applied to both sides of the comparison.</param>
/// <param name="Fail">Action when the stencil test fails.</param>
/// <param name="DepthFail">Action when the stencil test passes but the depth test fails.</param>
/// <param name="Pass">Action when both tests pass.</param>
public readonly record struct GlStencil(GlCompare Function, int Reference, int Mask, GlStencilOp Fail, GlStencilOp DepthFail, GlStencilOp Pass)
{
    /// <summary>OpenGL's initial state: always pass, reference 0, all bits, keep everything.</summary>
    public static GlStencil Default { get; } = new(GlCompare.Always, 0, -1, GlStencilOp.Keep, GlStencilOp.Keep, GlStencilOp.Keep);
}
