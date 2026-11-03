using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Saoss.Game.Rendering;

/// <summary>
/// Builds a static triangle mesh with an OpenGL 1.x style matrix stack and GLU-like primitives,
/// so the original display-list drawing code can be translated almost line by line.
/// Angles are in degrees, like <c>glRotatef</c>.
/// </summary>
public sealed class MeshBuilder
{
    private readonly List<VertexPositionNormalColor> _vertices = [];
    private readonly Stack<Matrix> _matrixStack = new();
    private Matrix _transform = Matrix.Identity;
    private Color _color = Color.White;

    /// <summary>Triangle-list vertices emitted so far, already transformed to model space.</summary>
    public IReadOnlyList<VertexPositionNormalColor> Vertices => _vertices;

    /// <summary>Saves the current transform (<c>glPushMatrix</c>).</summary>
    public void PushMatrix() => _matrixStack.Push(_transform);

    /// <summary>Restores the last saved transform (<c>glPopMatrix</c>).</summary>
    public void PopMatrix() => _transform = _matrixStack.Pop();

    /// <summary>Appends a translation to the current transform (<c>glTranslatef</c>).</summary>
    public void Translate(float x, float y, float z) => _transform = Matrix.CreateTranslation(x, y, z) * _transform;

    /// <summary>Appends a scale to the current transform (<c>glScalef</c>).</summary>
    public void Scale(float x, float y, float z) => _transform = Matrix.CreateScale(x, y, z) * _transform;

    /// <summary>Appends a rotation around an axis to the current transform (<c>glRotatef</c>).</summary>
    public void Rotate(float degrees, float x, float y, float z)
    {
        var axis = Vector3.Normalize(new Vector3(x, y, z));
        _transform = Matrix.CreateFromAxisAngle(axis, MathHelper.ToRadians(degrees)) * _transform;
    }

    /// <summary>Sets the color of subsequent vertices (<c>glColor4f</c>).</summary>
    public void SetColor(float r, float g, float b, float a = 1f) => _color = new Color(r, g, b, a);

    /// <summary>
    /// Emits a sphere centered at the origin with the same tessellation as <c>gluSphere</c>:
    /// <paramref name="slices"/> around the Z axis and <paramref name="stacks"/> from +Z to -Z.
    /// </summary>
    public void Sphere(float radius, int slices, int stacks)
    {
        var normalMatrix = NormalMatrix();
        for (var stack = 0; stack < stacks; stack++)
        {
            var rho0 = MathF.PI * stack / stacks;
            var rho1 = MathF.PI * (stack + 1) / stacks;
            for (var slice = 0; slice < slices; slice++)
            {
                var theta0 = 2f * MathF.PI * slice / slices;
                var theta1 = 2f * MathF.PI * (slice + 1) / slices;
                var a = SphereNormal(rho0, theta0);
                var b = SphereNormal(rho1, theta0);
                var c = SphereNormal(rho1, theta1);
                var d = SphereNormal(rho0, theta1);

                // The first and last stack meet at a pole, so one triangle covers the quad.
                if (stack > 0)
                {
                    AddSmoothTriangle(a, b, d, radius, normalMatrix);
                }

                if (stack < stacks - 1)
                {
                    AddSmoothTriangle(b, c, d, radius, normalMatrix);
                }
            }
        }
    }

    /// <summary>Uploads the emitted vertices to the GPU.</summary>
    public Mesh Build(GraphicsDevice graphicsDevice) => new(graphicsDevice, [.. _vertices]);

    private static Vector3 SphereNormal(float rho, float theta) =>
        new(-MathF.Sin(theta) * MathF.Sin(rho), MathF.Cos(theta) * MathF.Sin(rho), MathF.Cos(rho));

    // Normals use the inverse transpose and are renormalized, like GL_NORMALIZE.
    private Matrix NormalMatrix() => Matrix.Transpose(Matrix.Invert(_transform));

    private void AddSmoothTriangle(Vector3 n0, Vector3 n1, Vector3 n2, float radius, Matrix normalMatrix)
    {
        AddVertex(n0 * radius, n0, normalMatrix);
        AddVertex(n1 * radius, n1, normalMatrix);
        AddVertex(n2 * radius, n2, normalMatrix);
    }

    private void AddVertex(Vector3 position, Vector3 normal, Matrix normalMatrix)
    {
        var transformedNormal = Vector3.Normalize(Vector3.TransformNormal(normal, normalMatrix));
        _vertices.Add(new VertexPositionNormalColor(Vector3.Transform(position, _transform), transformedNormal, _color));
    }
}
