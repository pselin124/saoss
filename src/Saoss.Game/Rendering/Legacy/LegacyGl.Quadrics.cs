namespace Saoss.Game.Rendering.Legacy;

/// <summary>
/// GLU quadrics (gluSphere, gluCylinder, gluDisk, gluPartialDisk) with the same tessellation, vertex order
/// and normals as Mesa's GLU, so lighting and face culling match the original. Smooth normals,
/// filled style and outside orientation, the original's settings.
/// </summary>
public sealed partial class LegacyGl
{
    /// <summary>gluSphere around the Z axis; <paramref name="textured"/> matches gluQuadricTexture(GL_TRUE).</summary>
    public void Sphere(float radius, int slices, int stacks, bool textured = false)
    {
        if (TryRecord(g => g.Sphere(radius, slices, stacks, textured)))
        {
            return;
        }

        var dRho = MathF.PI / stacks;
        var dTheta = 2f * MathF.PI / slices;
        if (!textured)
        {
            SphereCaps(radius, slices, dRho, dTheta, top: true);
        }

        var first = textured ? 0 : 1;
        var last = textured ? stacks : stacks - 1;
        for (var i = first; i < last; i++)
        {
            var rho = i * dRho;
            Begin(GlPrimitive.QuadStrip);
            for (var j = 0; j <= slices; j++)
            {
                var theta = j == slices ? 0f : j * dTheta;
                var s = 1f - ((float)j / slices);
                SphereVertex(radius, theta, rho, s, 1f - ((float)i / stacks));
                SphereVertex(radius, theta, rho + dRho, s, 1f - ((float)(i + 1) / stacks));
            }

            End();
        }

        if (!textured)
        {
            SphereCaps(radius, slices, dRho, dTheta, top: false);
        }
    }

    /// <summary>gluCylinder along +Z from the base radius to the top radius.</summary>
    public void Cylinder(float baseRadius, float topRadius, float height, int slices, int stacks, bool textured = false)
    {
        if (TryRecord(g => g.Cylinder(baseRadius, topRadius, height, slices, stacks, textured)))
        {
            return;
        }

        var dAngle = 2f * MathF.PI / slices;
        var dRadius = (topRadius - baseRadius) / stacks;
        var dZ = height / stacks;
        var normalZ = (baseRadius - topRadius) / height;
        var radius = baseRadius;
        var z = 0f;
        for (var j = 0; j < stacks; j++)
        {
            Begin(GlPrimitive.QuadStrip);
            for (var i = 0; i <= slices; i++)
            {
                var angle = i == slices ? 0f : i * dAngle;
                var x = MathF.Sin(angle);
                var y = MathF.Cos(angle);
                var s = (float)i / slices;
                Normal(x, y, normalZ);
                if (textured)
                {
                    TexCoord(s, (float)j / stacks);
                }

                Vertex(x * radius, y * radius, z);
                Normal(x, y, normalZ);
                if (textured)
                {
                    TexCoord(s, (float)(j + 1) / stacks);
                }

                Vertex(x * (radius + dRadius), y * (radius + dRadius), z + dZ);
            }

            End();
            radius += dRadius;
            z += dZ;
        }
    }

    /// <summary>gluDisk in the XY plane facing +Z.</summary>
    public void Disk(float innerRadius, float outerRadius, int slices, int loops, bool textured = false) =>
        PartialDisk(innerRadius, outerRadius, slices, loops, 0f, 360f, textured);

    /// <summary>gluPartialDisk; angles in degrees, measured clockwise from +Y like GLU.</summary>
    public void PartialDisk(float innerRadius, float outerRadius, int slices, int loops, float startDegrees, float sweepDegrees, bool textured = false)
    {
        if (TryRecord(g => g.PartialDisk(innerRadius, outerRadius, slices, loops, startDegrees, sweepDegrees, textured)))
        {
            return;
        }

        var angleOffset = MathHelperRadians(startDegrees);
        var dAngle = MathHelperRadians(sweepDegrees) / slices;
        var dRadius = (outerRadius - innerRadius) / loops;
        Normal(0, 0, 1);
        for (var j = 0; j < loops; j++)
        {
            var radiusLow = outerRadius - (dRadius * j);
            var radiusHigh = radiusLow - dRadius;
            Begin(GlPrimitive.QuadStrip);
            for (var i = 0; i <= slices; i++)
            {
                var angle = angleOffset + (dAngle * i);
                var sin = MathF.Sin(angle);
                var cos = MathF.Cos(angle);
                if (textured)
                {
                    TexCoord((radiusLow / outerRadius / 2f * sin) + 0.5f, (radiusLow / outerRadius / 2f * cos) + 0.5f);
                }

                Vertex(radiusLow * sin, radiusLow * cos);
                if (textured)
                {
                    TexCoord((radiusHigh / outerRadius / 2f * sin) + 0.5f, (radiusHigh / outerRadius / 2f * cos) + 0.5f);
                }

                Vertex(radiusHigh * sin, radiusHigh * cos);
            }

            End();
        }
    }

    private static float MathHelperRadians(float degrees) => degrees / 180f * MathF.PI;

    private void SphereVertex(float radius, float theta, float rho, float s, float t)
    {
        var x = -MathF.Sin(theta) * MathF.Sin(rho);
        var y = MathF.Cos(theta) * MathF.Sin(rho);
        var z = MathF.Cos(rho);
        Normal(x, y, z);
        TexCoord(s, t);
        Vertex(x * radius, y * radius, z * radius);
    }

    // Untextured GLU spheres close the poles with triangle fans.
    private void SphereCaps(float radius, int slices, float dRho, float dTheta, bool top)
    {
        Begin(GlPrimitive.TriangleFan);
        Normal(0, 0, top ? 1 : -1);
        Vertex(0, 0, top ? radius : -radius);
        var rho = top ? dRho : MathF.PI - dRho;
        if (top)
        {
            for (var j = 0; j <= slices; j++)
            {
                FanVertex(j);
            }
        }
        else
        {
            for (var j = slices; j >= 0; j--)
            {
                FanVertex(j);
            }
        }

        End();

        void FanVertex(int j)
        {
            var theta = j == slices ? 0f : j * dTheta;
            var x = -MathF.Sin(theta) * MathF.Sin(rho);
            var y = MathF.Cos(theta) * MathF.Sin(rho);
            var z = MathF.Cos(rho);
            Normal(x, y, z);
            Vertex(x * radius, y * radius, z * radius);
        }
    }
}
