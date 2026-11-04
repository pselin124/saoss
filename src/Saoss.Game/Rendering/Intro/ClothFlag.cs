using Microsoft.Xna.Framework;

namespace Saoss.Game.Rendering.Intro;

/// <summary>
/// The intro's waving flag, a port of the mass-spring cloth in lippu.cpp (<c>alustaLippu</c>, <c>calculate</c>):
/// a grid of vertices joined by straight and diagonal springs, pulled by gravity and by wind on each triangle, with
/// the left corners tied to the pole by cubic springs and every vertex kept outside the pole. One <see cref="Step(int)"/>
/// is one original intro frame (150 per second).
/// </summary>
/// <remarks>
/// The cloth is chaotic, and a flag wound tightly round the pole can stay frozen there for good (stopped vertices
/// only move when a spring tugs them off the pole). Which way it goes depends on rounding, so the arithmetic follows
/// the original's x87 build: values are stored as floats but every expression is evaluated in double precision. Done
/// with float math instead, the flag winds up half a minute in and never comes loose.
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
    private const double PoleClearance = 0.3;
    private const float PoleSnapRadius = 0.29f;
    private const int MaxTrianglesPerVertex = 6;
    private const float WindAngleFrameOffset = 600f;
    private const float WindAngleFramePeriod = 400f;
    private const float WindStrengthFramePeriod = 1000f;
    private const float WindStrengthPhase = 1f;
    private const double WindStrength = 0.001;
    private const double WindScale = 0.2;

    private static readonly double RestLength = FlagWidth / Columns;
    private static readonly double DiagonalRestLength = Math.Sqrt(2) * RestLength;

    private readonly Vector3[] _positions = new Vector3[Columns * Rows];
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
    /// velocity and position update, normals.
    /// </summary>
    public void Step(Vector3 wind)
    {
        ApplyWind(wind);
        for (var i = 0; i < _positions.Length; i++)
        {
            ApplyVertexForces(i);
        }

        Integrate();
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

    private void ApplySpring(int i, int neighbor, double restLength)
    {
        var offset = _positions[i] - _positions[neighbor];
        var distance = (float)Math.Sqrt(((double)offset.X * offset.X) + ((double)offset.Y * offset.Y) + ((double)offset.Z * offset.Z));
        var inverseDistance = (float)(1.0 / distance);
        var stretch = (float)((distance - restLength) / restLength);
        var direction = offset * inverseDistance;
        if (distance != 0)
        {
            var force = new Vector3(
                (float)((double)direction.X * stretch * SpringStiffness),
                (float)((double)direction.Y * stretch * SpringStiffness),
                (float)((double)direction.Z * stretch * SpringStiffness));
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

    // Vertices that would end inside the pole are put on its surface and stopped. The clearance is measured with the
    // force added instead of the velocity, as in the original.
    private void Integrate()
    {
        for (var i = 0; i < _positions.Length; i++)
        {
            var x = (double)_positions[i].X + _forces[i].X;
            var z = (double)_positions[i].Z + _forces[i].Z;
            var clearance = (float)Math.Sqrt((x * x) + (z * z));
            _velocities[i] += _forces[i];
            _forces[i] = Vector3.Zero;
            if (clearance > PoleClearance)
            {
                _positions[i] += _velocities[i];
            }
            else
            {
                _positions[i].X = (float)(((double)_positions[i].X + _velocities[i].X) / clearance * PoleSnapRadius);
                _positions[i].Z = (float)(((double)_positions[i].Z + _velocities[i].Z) / clearance * PoleSnapRadius);
                _velocities[i] = Vector3.Zero;
            }
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
