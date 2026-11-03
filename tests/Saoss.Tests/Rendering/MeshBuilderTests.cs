using Microsoft.Xna.Framework;
using Saoss.Game.Rendering;

namespace Saoss.Tests.Rendering;

/// <summary>
/// Tests that <see cref="MeshBuilder"/> reproduces GLU sphere geometry and OpenGL matrix-stack semantics.
/// </summary>
[TestClass]
public sealed class MeshBuilderTests
{
    private const float Tolerance = 1e-4f;

    [TestMethod]
    public void Sphere_EmitsGluTriangleCount()
    {
        var builder = new MeshBuilder();

        builder.Sphere(1f, slices: 8, stacks: 5);

        // Two triangles per quad, minus one per slice at each pole.
        var expectedTriangles = (2 * 8 * 5) - (2 * 8);
        Assert.AreEqual(expectedTriangles * 3, builder.Vertices.Count);
    }

    [TestMethod]
    public void Sphere_VerticesLieOnRadiusWithOutwardUnitNormals()
    {
        var builder = new MeshBuilder();

        builder.Sphere(2.5f, slices: 12, stacks: 7);

        foreach (var vertex in builder.Vertices)
        {
            Assert.AreEqual(2.5f, vertex.Position.Length(), Tolerance);
            Assert.AreEqual(1f, vertex.Normal.Length(), Tolerance);
            Assert.AreEqual(1f, Vector3.Dot(Vector3.Normalize(vertex.Position), vertex.Normal), Tolerance);
        }
    }

    [TestMethod]
    public void Transforms_ApplyInGlOrder_LastCallAffectsGeometryFirst()
    {
        var builder = new MeshBuilder();

        // glTranslatef(10,0,0); glScalef(2,1,1); => x' = 10 + 2x
        builder.Translate(10f, 0f, 0f);
        builder.Scale(2f, 1f, 1f);
        builder.Sphere(1f, slices: 4, stacks: 2);

        var maxX = builder.Vertices.Max(v => v.Position.X);
        var minX = builder.Vertices.Min(v => v.Position.X);
        Assert.AreEqual(12f, maxX, Tolerance);
        Assert.AreEqual(8f, minX, Tolerance);
    }

    [TestMethod]
    public void Rotate_UsesCounterClockwiseDegreesLikeGlRotate()
    {
        var builder = new MeshBuilder();

        // Rotating +90 degrees about Z moves +X to +Y.
        builder.Rotate(90f, 0f, 0f, 1f);
        builder.Translate(5f, 0f, 0f);
        builder.Sphere(0.001f, slices: 4, stacks: 2);

        var center = builder.Vertices.Aggregate(Vector3.Zero, (sum, v) => sum + v.Position) / builder.Vertices.Count;
        Assert.AreEqual(0f, center.X, 1e-3f);
        Assert.AreEqual(5f, center.Y, 1e-3f);
    }

    [TestMethod]
    public void NonUniformScale_KeepsNormalsUnitLengthAndPerpendicularToSurface()
    {
        var builder = new MeshBuilder();

        builder.Scale(4f, 1f, 1f);
        builder.Sphere(1f, slices: 16, stacks: 8);

        foreach (var vertex in builder.Vertices)
        {
            Assert.AreEqual(1f, vertex.Normal.Length(), Tolerance);

            // Ellipsoid x^2/16 + y^2 + z^2 = 1 has gradient (x/16, y, z).
            var p = vertex.Position;
            var gradient = Vector3.Normalize(new Vector3(p.X / 16f, p.Y, p.Z));
            Assert.AreEqual(1f, Vector3.Dot(gradient, vertex.Normal), Tolerance);
        }
    }

    [TestMethod]
    public void PopMatrix_RestoresPreviousTransform()
    {
        var builder = new MeshBuilder();

        builder.PushMatrix();
        builder.Translate(100f, 0f, 0f);
        builder.PopMatrix();
        builder.Sphere(1f, slices: 4, stacks: 2);

        Assert.IsTrue(builder.Vertices.All(v => v.Position.Length() <= 1f + Tolerance));
    }
}
