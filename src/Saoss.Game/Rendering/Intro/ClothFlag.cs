using Microsoft.Xna.Framework;

namespace Saoss.Game.Rendering.Intro;

/// <summary>
/// The intro's waving flag, based on the mass-spring cloth in lippu.cpp (<c>alustaLippu</c>, <c>calculate</c>):
/// a grid of vertices joined by straight and diagonal springs, pulled by gravity and by wind on each triangle, with
/// the left corners tied to the pole by cubic springs. One <see cref="Step(int)"/> is one original intro frame (150 per
/// second).
/// </summary>
/// <remarks>
/// Additions to the original, which let the soft springs stretch far in strong wind (and blow apart in a storm) and
/// froze every vertex touching the pole, so the flag could wind round it for good: weak bending springs over two grid
/// steps for broad folds; after each step, edges are kept within 10% of their length, the corners within rope's
/// reach of their tie points and the cloth outside the pole and its knob; and velocities are taken from the
/// corrected motion, with friction while sliding on the pole. The original's arithmetic (float storage, double
/// precision expressions as in its x87 build) is kept.
/// </remarks>
public sealed class ClothFlag
{
    /// <summary>Vertices per row [LIPUN_X_VERTEXIT].</summary>
    public const int Columns = 35;

    /// <summary>Vertex rows [LIPUN_Y_VERTEXIT].</summary>
    public const int Rows = 25;

    /// <summary>Height of the pole top, where the flag's top edge is tied [TANGON_KORKEUS].</summary>
    public const float PoleHeight = 15f;

    /// <summary>Index of the top-left vertex, tied to the pole top.</summary>
    public const int TopLeft = 0;

    /// <summary>Index of the bottom-left vertex, tied lower down the pole.</summary>
    public const int BottomLeft = (Rows - 1) * Columns;

    private const float FlagWidth = 6f;
    private const float FlagHeight = 4.5f;
    private const float StartX = 0.2f;
    private const float StartDrop = 0.3f;
    private const float SpringStiffness = 0.055f;
    private const double Gravity = 0.000058;
    private const float Damping = 1.00185f;
    private const double AnchorSlack = 0.3;
    private const double AnchorStiffness = 0.2;
    private const double BottomAnchorExtraDrop = 1.3;
    private const float BendStiffness = 0.04f;
    private const float MaxStretch = 1.1f;
    private const float HardMaxStretch = 1.25f;
    private const int ConstraintPasses = 4;
    private const float TopRopeLength = 0.35f;
    private const float TopTieDrop = 0.3f;
    private const float BottomRopeLength = 1.0f;
    private const float PoleContactRadius = 0.23f;
    private const float KnobContactRadius = 0.38f;
    private const float PoleFriction = 0.1f;
    private const int MaxTrianglesPerVertex = 6;
    private const float WindAngleFrameOffset = 600f;
    private const float WindAngleFramePeriod = 400f;
    private const float WindStrengthFramePeriod = 1000f;
    private const float WindStrengthPhase = 1f;
    private const double WindStrength = 0.001;
    private const double WindScale = 0.2;

    private static readonly double RestLength = FlagWidth / Columns;
    private static readonly double DiagonalRestLength = Math.Sqrt(2) * RestLength;
    private static readonly double BendRestLength = 2 * RestLength;
    private static readonly float MaxEdgeLength = (float)RestLength * MaxStretch;
    private static readonly float HardMaxEdgeLength = (float)RestLength * HardMaxStretch;
    private static readonly Vector3 TopTie = new(0, PoleHeight - TopTieDrop, 0);
    private static readonly Vector3 BottomAnchor = new(0, (float)(PoleHeight - FlagHeight - BottomAnchorExtraDrop), 0);
    private static readonly Vector3 KnobCenter = new(0, PoleHeight, 0);

    private readonly Vector3[] _positions = new Vector3[Columns * Rows];
    private readonly Vector3[] _previous = new Vector3[Columns * Rows];
    private readonly Vector3[] _poleContacts = new Vector3[Columns * Rows];
    private readonly Vector3[] _forces = new Vector3[Columns * Rows];
    private readonly Vector3[] _velocities = new Vector3[Columns * Rows];
    private readonly Vector3[] _normals = new Vector3[Columns * Rows];
    private readonly Vector2[] _texCoords = new Vector2[Columns * Rows];
    private readonly int[][] _neighbors = new int[Columns * Rows][];
    private readonly int[][] _diagonalNeighbors = new int[Columns * Rows][];
    private readonly List<int>[] _vertexTriangles = new List<int>[Columns * Rows];
    private readonly int[] _triangleCorners = new int[(Columns - 1) * (Rows - 1) * 2 * 3];
    private readonly Vector3[] _triangleNormals = new Vector3[(Columns - 1) * (Rows - 1) * 2];

    /// <summary>Lays the flag out flat beside the pole top, like <c>alustaLippu</c>, and computes its normals.</summary>
    public ClothFlag()
    {
        for (var row = 0; row < Rows; row++)
        {
            for (var column = 0; column < Columns; column++)
            {
                var i = (row * Columns) + column;
                _positions[i] = new Vector3(
                    StartX + ((float)column / Columns * FlagWidth),
                    PoleHeight - StartDrop - ((float)row / Rows * FlagHeight),
                    0);
                _texCoords[i] = new Vector2((float)column / (Columns - 1), (float)row / (Rows - 1));
                (_neighbors[i], _diagonalNeighbors[i]) = FindNeighbors(row, column);
                _vertexTriangles[i] = [];
            }
        }

        BuildTriangles();
        UpdateNormals();
    }

    /// <summary>Vertex positions, row by row from the top-left corner.</summary>
    public IReadOnlyList<Vector3> Positions => _positions;

    /// <summary>Vertex normals: the average of the adjacent triangles' unit normals (not renormalized).</summary>
    public IReadOnlyList<Vector3> Normals => _normals;

    /// <summary>Texture coordinates; (0, 0) is the top-left corner of the picture.</summary>
    public IReadOnlyList<Vector2> TexCoords => _texCoords;

    /// <summary>Three vertex indices per triangle, in the original's drawing order.</summary>
    public IReadOnlyList<int> TriangleCorners => _triangleCorners;

    /// <summary>
    /// The wind of intro frame <paramref name="frame"/>: its direction swings slowly around the Y axis and its strength
    /// rises and falls (and reverses) over thousands of frames [tuulikulma, tuulenvoimakkuus].
    /// </summary>
    public static Vector3 WindAt(int frame)
    {
        var angle = (float)Math.Sin((frame - WindAngleFrameOffset) / WindAngleFramePeriod);
        var strength = (float)(WindStrength * Math.Sin((frame / WindStrengthFramePeriod) - WindStrengthPhase));
        return new Vector3((float)(Math.Sin(angle) * strength * WindScale), 0, (float)(Math.Cos(angle) * strength * WindScale));
    }

    /// <summary>Advances the cloth by intro frame <paramref name="frame"/> with that frame's wind [calculate].</summary>
    public void Step(int frame) => Step(WindAt(frame));

    /// <summary>
    /// Advances the cloth by one frame. Steps: wind force on every triangle, springs and anchors per vertex,
    /// position update, constraints (stretch, corner ropes, pole), velocities from the actual motion, normals.
    /// </summary>
    public void Step(Vector3 wind)
    {
        ApplyWind(wind);
        for (var i = 0; i < _positions.Length; i++)
        {
            ApplyVertexForces(i);
        }

        Integrate();
        SatisfyConstraints();
        UpdateVelocities();
        UpdateNormals();
    }

    // Each triangle pushes its corners along its normal, scaled by how squarely it faces the wind.
    private void ApplyWind(Vector3 wind)
    {
        for (var t = 0; t < _triangleNormals.Length; t++)
        {
            var normal = _triangleNormals[t];
            var facing = (float)(((double)normal.X * wind.X) + ((double)normal.Y * wind.Y) + ((double)normal.Z * wind.Z));
            var force = normal * facing;
            for (var corner = 0; corner < 3; corner++)
            {
                _forces[_triangleCorners[(t * 3) + corner]] += force;
            }
        }
    }

    // Springs are evaluated from half of the vertices (a checkerboard for straight ones, even columns for diagonal
    // ones), as in the original, so some diagonal pairs are counted twice and others once.
    private void ApplyVertexForces(int i)
    {
        var column = i % Columns;
        var row = i / Columns;
        _forces[i].Y = (float)(_forces[i].Y - Gravity);
        if ((row + column) % 2 == 0)
        {
            foreach (var neighbor in _neighbors[i])
            {
                ApplySpring(i, neighbor, RestLength);
            }
        }

        if (column % 2 == 0)
        {
            foreach (var neighbor in _diagonalNeighbors[i])
            {
                ApplySpring(i, neighbor, DiagonalRestLength);
            }
        }

        // Weak springs over two grid steps resist bending, so the cloth folds in broad waves instead of crumpling.
        if (column + 2 < Columns)
        {
            ApplySpring(i, i + 2, BendRestLength, BendStiffness);
        }

        if (row + 2 < Rows)
        {
            ApplySpring(i, i + (2 * Columns), BendRestLength, BendStiffness);
        }

        if (i == TopLeft)
        {
            ApplyAnchor(i, PoleHeight);
        }
        else if (i == BottomLeft)
        {
            ApplyAnchor(i, PoleHeight - FlagHeight - BottomAnchorExtraDrop);
        }

        // Divided per component: Vector3's division multiplies by the reciprocal, which rounds differently.
        _velocities[i].X /= Damping;
        _velocities[i].Y /= Damping;
        _velocities[i].Z /= Damping;
    }

    private void ApplySpring(int i, int neighbor, double restLength, float stiffness = SpringStiffness)
    {
        var offset = _positions[i] - _positions[neighbor];
        var distance = (float)Math.Sqrt(((double)offset.X * offset.X) + ((double)offset.Y * offset.Y) + ((double)offset.Z * offset.Z));
        var inverseDistance = (float)(1.0 / distance);
        var stretch = (float)((distance - restLength) / restLength);
        var direction = offset * inverseDistance;
        if (distance != 0)
        {
            var force = new Vector3(
                (float)((double)direction.X * stretch * stiffness),
                (float)((double)direction.Y * stretch * stiffness),
                (float)((double)direction.Z * stretch * stiffness));
            _forces[i] -= force;
            _forces[neighbor] += force;
        }
    }

    // A slack cubic spring pulls the corner back toward its point on the pole axis.
    private void ApplyAnchor(int i, double anchorHeight)
    {
        var offset = new Vector3(_positions[i].X, (float)(_positions[i].Y - anchorHeight), _positions[i].Z);
        var distance = Math.Sqrt(((double)offset.X * offset.X) + ((double)offset.Y * offset.Y) + ((double)offset.Z * offset.Z));
        if (distance > AnchorSlack)
        {
            var pull = Math.Pow(distance - AnchorSlack, 3) * AnchorStiffness;
            _forces[i] = new Vector3(
                (float)(_forces[i].X - (offset.X * pull)),
                (float)(_forces[i].Y - (offset.Y * pull)),
                (float)(_forces[i].Z - (offset.Z * pull)));
        }
    }

    // Moves every vertex by its velocity, remembering where it was so the constraints' corrections become motion too.
    private void Integrate()
    {
        for (var i = 0; i < _positions.Length; i++)
        {
            _velocities[i] += _forces[i];
            _forces[i] = Vector3.Zero;
            _previous[i] = _positions[i];
            _positions[i] += _velocities[i];
        }
    }

    // Constraints are projected a few times in turn, so each mostly holds after the others have moved the vertices.
    private void SatisfyConstraints()
    {
        for (var pass = 0; pass < ConstraintPasses; pass++)
        {
            LimitStretch();
            if (pass == 0)
            {
                CapRowStretch();
            }

            TieCorner(TopLeft, TopTie, TopRopeLength);
            TieCorner(BottomLeft, BottomAnchor, BottomRopeLength);
            KeepOffPole();
        }
    }

    // Edges stretched beyond the limit are pulled back from both ends (Provot), so strong wind cannot draw the soft
    // springs out; real cloth hardly stretches.
    private void LimitStretch()
    {
        for (var row = 0; row < Rows; row++)
        {
            for (var column = 0; column < Columns; column++)
            {
                var i = (row * Columns) + column;
                if (column + 1 < Columns)
                {
                    LimitEdge(i, i + 1, MaxEdgeLength, outerOnly: false);
                }

                if (row + 1 < Rows)
                {
                    LimitEdge(i, i + Columns, MaxEdgeLength, outerOnly: false);
                }
            }
        }
    }

    // A correction from both ends needs a pass per edge to reach across the flag, so in a storm the rows could still
    // stretch far. A sweep out from the pole moving only the outer vertex caps each row in one go; it is kept for
    // extreme stretch because it also brakes the fly, twisting the cloth flat if used all the time.
    private void CapRowStretch()
    {
        for (var row = 0; row < Rows; row++)
        {
            for (var column = 0; column + 1 < Columns; column++)
            {
                var i = (row * Columns) + column;
                LimitEdge(i, i + 1, HardMaxEdgeLength, outerOnly: true);
            }
        }
    }

    private void LimitEdge(int a, int b, float maxLength, bool outerOnly)
    {
        var offset = _positions[b] - _positions[a];
        var length = offset.Length();
        if (length > maxLength)
        {
            var excess = offset * ((length - maxLength) / length);
            if (outerOnly)
            {
                _positions[b] -= excess;
            }
            else
            {
                _positions[a] += excess * 0.5f;
                _positions[b] -= excess * 0.5f;
            }
        }
    }

    // The corner cannot pull its rope further than the rope's length from the tie point.
    private void TieCorner(int i, Vector3 anchor, float ropeLength)
    {
        var offset = _positions[i] - anchor;
        var length = offset.Length();
        if (length > ropeLength)
        {
            _positions[i] = anchor + (offset * (ropeLength / length));
        }
    }

    // Vertices inside the pole or its knob are pushed out to the surface (plus a little for the cloth's thickness).
    private void KeepOffPole()
    {
        for (var i = 0; i < _positions.Length; i++)
        {
            var position = _positions[i];
            var fromKnob = position - KnobCenter;
            var knobDistance = fromKnob.Length();
            if (knobDistance < KnobContactRadius)
            {
                var away = knobDistance > 0 ? fromKnob / knobDistance : Vector3.UnitX;
                position = KnobCenter + (away * KnobContactRadius);
                _positions[i] = position;
                _poleContacts[i] = away;
            }

            // Pushing straight out from the axis only moves the vertex further from the knob, so both hold.
            var axisDistance = MathF.Sqrt((position.X * position.X) + (position.Z * position.Z));
            if (position.Y < PoleHeight && axisDistance < PoleContactRadius)
            {
                var away = axisDistance > 0 ? new Vector3(position.X / axisDistance, 0, position.Z / axisDistance) : Vector3.UnitX;
                _positions[i] = new Vector3(away.X * PoleContactRadius, position.Y, away.Z * PoleContactRadius);
                _poleContacts[i] = away;
            }
        }
    }

    // The velocity is what the vertex really moved. Against the pole it keeps moving away freely, but sliding along
    // the pole is slowed by friction, so the cloth clings for a moment and then peels off.
    private void UpdateVelocities()
    {
        for (var i = 0; i < _positions.Length; i++)
        {
            var velocity = _positions[i] - _previous[i];
            var away = _poleContacts[i];
            if (away != Vector3.Zero)
            {
                var outward = Vector3.Dot(velocity, away);
                var slide = velocity - (away * outward);
                velocity = (away * MathF.Max(outward, 0)) + (slide * (1 - PoleFriction));
                _poleContacts[i] = Vector3.Zero;
            }

            _velocities[i] = velocity;
        }
    }

    private void UpdateNormals()
    {
        for (var t = 0; t < _triangleNormals.Length; t++)
        {
            var origin = _positions[_triangleCorners[t * 3]];
            var edge1 = _positions[_triangleCorners[(t * 3) + 1]] - origin;
            var edge2 = _positions[_triangleCorners[(t * 3) + 2]] - origin;
            var cross = new Vector3(
                (float)(((double)edge1.Y * edge2.Z) - ((double)edge1.Z * edge2.Y)),
                (float)(((double)edge1.Z * edge2.X) - ((double)edge1.X * edge2.Z)),
                (float)(((double)edge1.X * edge2.Y) - ((double)edge1.Y * edge2.X)));
            var length = (float)Math.Sqrt(((double)cross.X * cross.X) + ((double)cross.Y * cross.Y) + ((double)cross.Z * cross.Z));
            _triangleNormals[t] = new Vector3(cross.X / length, cross.Y / length, cross.Z / length);
        }

        for (var i = 0; i < _normals.Length; i++)
        {
            var sum = Vector3.Zero;
            foreach (var t in _vertexTriangles[i])
            {
                sum += _triangleNormals[t];
            }

            _normals[i] = sum / _vertexTriangles[i].Count;
        }
    }

    // Two triangles per grid cell, row by row.
    private void BuildTriangles()
    {
        var t = 0;
        for (var row = 0; row < Rows - 1; row++)
        {
            for (var column = 0; column < Columns - 1; column++)
            {
                var topLeft = (row * Columns) + column;
                var bottomLeft = topLeft + Columns;
                AddTriangle(t++, topLeft, bottomLeft, topLeft + 1);
                AddTriangle(t++, bottomLeft + 1, topLeft + 1, bottomLeft);
            }
        }
    }

    private void AddTriangle(int triangle, int a, int b, int c)
    {
        _triangleCorners[triangle * 3] = a;
        _triangleCorners[(triangle * 3) + 1] = b;
        _triangleCorners[(triangle * 3) + 2] = c;
        foreach (var corner in (ReadOnlySpan<int>)[a, b, c])
        {
            if (_vertexTriangles[corner].Count < MaxTrianglesPerVertex)
            {
                _vertexTriangles[corner].Add(triangle);
            }
        }
    }

    // The grid neighbors that exist: up, left, right, down; and diagonally up-left, up-right, down-left, down-right.
    private static (int[] Straight, int[] Diagonal) FindNeighbors(int row, int column)
    {
        var straight = new List<int>();
        var diagonal = new List<int>();
        void Add(List<int> list, int r, int c)
        {
            if (r is >= 0 and < Rows && c is >= 0 and < Columns)
            {
                list.Add((r * Columns) + c);
            }
        }

        Add(straight, row - 1, column);
        Add(straight, row, column - 1);
        Add(straight, row, column + 1);
        Add(straight, row + 1, column);
        Add(diagonal, row - 1, column - 1);
        Add(diagonal, row - 1, column + 1);
        Add(diagonal, row + 1, column - 1);
        Add(diagonal, row + 1, column + 1);
        return ([.. straight], [.. diagonal]);
    }
}
