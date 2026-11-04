using System.Runtime.InteropServices;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using XnaColor = Microsoft.Xna.Framework.Color;

namespace Saoss.Game.Rendering.Legacy;

/// <summary>
/// A small OpenGL 1.x fixed-function emulation on top of MonoGame, so the original drawing code can be
/// translated call by call. Vertices are transformed to eye space on the CPU and batched by render state; lit
/// vertices are lit on the CPU (including point lights) when the batches are drawn, by <see cref="Flush"/>, with a
/// <see cref="BasicEffect"/>.
/// Method names follow GL without the <c>gl</c> prefix; angles are degrees. Unlike OpenGL, GL_NORMALIZE starts
/// enabled, the state the game's own setup left for everything but the intro.
/// </summary>
public sealed partial class LegacyGl : IDisposable
{
    // Job index of a vertex whose color is final.
    private const int Unlit = -1;
    private const int ColorChunkSize = 16384;
    private const int ParallelColorThreshold = 2 * ColorChunkSize;

    private readonly GraphicsDevice _device;
    private readonly BasicEffect _effect;
    private readonly Stack<Matrix> _modelViewStack = new();
    private readonly Stack<Matrix> _projectionStack = new();
    private readonly List<VertexPositionColorTexture> _primitive = [];
    private readonly List<int> _primitiveJobs = [];
    private readonly Dictionary<(GlBlend, GlBlend), BlendState> _blendStates = [];
    private readonly DepthStencilState _depthTestState = new() { DepthBufferEnable = true, DepthBufferWriteEnable = true, DepthBufferFunction = CompareFunction.LessEqual };
    private readonly GlDeferredShading _shading = new();
    private readonly List<GlDrawRange> _ranges = [];
    private readonly Action<int> _resolveColorChunk;

    private Matrix _modelView = Matrix.Identity;
    private Matrix _projection = Matrix.Identity;
    private Matrix _normalMatrix = Matrix.Identity;
    private bool _isNormalMatrixDirty;
    private GlMatrixMode _matrixMode = GlMatrixMode.ModelView;

    private bool _blend;
    private bool _depthTest;
    private bool _texture2D;
    private bool _cullFace;
    private bool _lighting;
    private bool _normalize = true;
    private bool _frontFaceClockwise;
    private GlBlend _sourceBlend = GlBlend.One;
    private GlBlend _destinationBlend = GlBlend.Zero;
    private Texture2D? _boundTexture;
    private Vector4 _color = Vector4.One;
    private Vector3 _normal = Vector3.UnitZ;
    private Vector2 _texCoord;
    private XnaColor _clearColor = XnaColor.Black;
    private GlPrimitive? _currentPrimitive;
    private int _primitiveBackCount;

    private VertexPositionColorTexture[] _batch = new VertexPositionColorTexture[4096];
    private int[] _batchJobs = new int[4096];
    private int _batchCount;
    private int[] _indices = new int[8192];
    private int _indexCount;
    private int _rangeStart;
    private int _primitiveBase;
    private int _primitiveBackBase;
    private GlBatchKey _batchKey;
    private DynamicVertexBuffer? _vertexBuffer;
    private DynamicIndexBuffer? _indexBuffer;

    private GlDisplayList? _recording;

    /// <summary>Creates the emulation for the given device.</summary>
    public LegacyGl(GraphicsDevice device)
    {
        _device = device;
        _effect = new BasicEffect(device) { LightingEnabled = false, VertexColorEnabled = true, World = Matrix.Identity, View = Matrix.Identity };
        _resolveColorChunk = ResolveColorChunk;
    }

    /// <summary>Fixed-function lighting state (lights, material, light model).</summary>
    public GlLighting Lighting { get; } = new();

    /// <summary>Width of the current viewport in pixels.</summary>
    public int ViewportWidth => _device.Viewport.Width;

    /// <summary>Height of the current viewport in pixels.</summary>
    public int ViewportHeight => _device.Viewport.Height;

    /// <summary>Current modelview matrix (row-vector convention).</summary>
    public Matrix ModelView => _modelView;

    // ---- Matrices -------------------------------------------------------------------------------

    /// <summary>glMatrixMode.</summary>
    public void MatrixMode(GlMatrixMode mode)
    {
        if (TryRecord(mode, static (g, m) => g.MatrixMode(m)))
        {
            return;
        }

        _matrixMode = mode;
    }

    /// <summary>glLoadIdentity.</summary>
    public void LoadIdentity()
    {
        if (TryRecord(static g => g.LoadIdentity()))
        {
            return;
        }

        SetCurrentMatrix(Matrix.Identity);
    }

    /// <summary>glPushMatrix.</summary>
    public void PushMatrix()
    {
        if (TryRecord(static g => g.PushMatrix()))
        {
            return;
        }

        (_matrixMode == GlMatrixMode.ModelView ? _modelViewStack : _projectionStack).Push(CurrentMatrix);
    }

    /// <summary>glPopMatrix.</summary>
    public void PopMatrix()
    {
        if (TryRecord(static g => g.PopMatrix()))
        {
            return;
        }

        var stack = _matrixMode == GlMatrixMode.ModelView ? _modelViewStack : _projectionStack;
        if (stack.Count > 0)
        {
            SetCurrentMatrix(stack.Pop());
        }
    }

    /// <summary>glTranslatef.</summary>
    public void Translate(float x, float y, float z)
    {
        if (TryRecord((x, y, z), static (g, a) => g.Translate(a.x, a.y, a.z)))
        {
            return;
        }

        MultiplyCurrent(Matrix.CreateTranslation(x, y, z));
    }

    /// <summary>glRotatef: counter-clockwise degrees around the axis.</summary>
    public void Rotate(float degrees, float x, float y, float z)
    {
        if (TryRecord((degrees, x, y, z), static (g, a) => g.Rotate(a.degrees, a.x, a.y, a.z)))
        {
            return;
        }

        var axis = new Vector3(x, y, z);
        if (axis.LengthSquared() == 0)
        {
            return;
        }

        MultiplyCurrent(Matrix.CreateFromAxisAngle(Vector3.Normalize(axis), MathHelper.ToRadians(degrees)));
    }

    /// <summary>glScalef.</summary>
    public void Scale(float x, float y, float z)
    {
        if (TryRecord((x, y, z), static (g, a) => g.Scale(a.x, a.y, a.z)))
        {
            return;
        }

        MultiplyCurrent(Matrix.CreateScale(x, y, z));
    }

    /// <summary>gluPerspective, without argument validation (the original sometimes passes odd planes).</summary>
    public void Perspective(float fovYDegrees, float aspect, float near, float far)
    {
        if (TryRecord((fovYDegrees, aspect, near, far), static (g, a) => g.Perspective(a.fovYDegrees, a.aspect, a.near, a.far)))
        {
            return;
        }

        var f = 1f / MathF.Tan(MathHelper.ToRadians(fovYDegrees) / 2f);
        var depth = near - far;
        var matrix = new Matrix(
            f / aspect, 0, 0, 0,
            0, f, 0, 0,
            0, 0, far / depth, -1,
            0, 0, near * far / depth, 0);
        MultiplyCurrent(matrix);
    }

    /// <summary>glOrtho.</summary>
    public void Ortho(float left, float right, float bottom, float top, float near, float far)
    {
        if (TryRecord((left, right, bottom, top, near, far), static (g, a) => g.Ortho(a.left, a.right, a.bottom, a.top, a.near, a.far)))
        {
            return;
        }

        var matrix = new Matrix(
            2f / (right - left), 0, 0, 0,
            0, 2f / (top - bottom), 0, 0,
            0, 0, 1f / (near - far), 0,
            (left + right) / (left - right), (top + bottom) / (bottom - top), near / (near - far), 1);
        MultiplyCurrent(matrix);
    }

    /// <summary>gluLookAt.</summary>
    public void LookAt(float eyeX, float eyeY, float eyeZ, float centerX, float centerY, float centerZ, float upX, float upY, float upZ)
    {
        if (TryRecord(
            (eyeX, eyeY, eyeZ, centerX, centerY, centerZ, upX, upY, upZ),
            static (g, a) => g.LookAt(a.eyeX, a.eyeY, a.eyeZ, a.centerX, a.centerY, a.centerZ, a.upX, a.upY, a.upZ)))
        {
            return;
        }

        var eye = new Vector3(eyeX, eyeY, eyeZ);
        var forward = Vector3.Normalize(new Vector3(centerX, centerY, centerZ) - eye);
        var side = Vector3.Normalize(Vector3.Cross(forward, new Vector3(upX, upY, upZ)));
        var up = Vector3.Cross(side, forward);
        var rotation = new Matrix(
            side.X, up.X, -forward.X, 0,
            side.Y, up.Y, -forward.Y, 0,
            side.Z, up.Z, -forward.Z, 0,
            0, 0, 0, 1);
        MultiplyCurrent(Matrix.CreateTranslation(-eye) * rotation);
    }

    // ---- State ----------------------------------------------------------------------------------

    /// <summary>glEnable.</summary>
    public void Enable(GlCapability capability) => SetCapability(capability, true);

    /// <summary>glDisable.</summary>
    public void Disable(GlCapability capability) => SetCapability(capability, false);

    /// <summary>glBlendFunc.</summary>
    public void BlendFunc(GlBlend source, GlBlend destination)
    {
        if (TryRecord((source, destination), static (g, a) => g.BlendFunc(a.source, a.destination)))
        {
            return;
        }

        _sourceBlend = source;
        _destinationBlend = destination;
    }

    /// <summary>glFrontFace: true for GL_CW.</summary>
    public void FrontFaceClockwise(bool clockwise)
    {
        if (TryRecord(clockwise, static (g, c) => g.FrontFaceClockwise(c)))
        {
            return;
        }

        _frontFaceClockwise = clockwise;
    }

    /// <summary>glBindTexture; null unbinds (texturing then has no effect).</summary>
    public void BindTexture(Texture2D? texture)
    {
        if (TryRecord(texture, static (g, t) => g.BindTexture(t)))
        {
            return;
        }

        _boundTexture = texture;
    }

    /// <summary>glColor4f.</summary>
    public void Color(float r, float g, float b, float a = 1f)
    {
        if (TryRecord(GlListOp.Color, r, g, b, a))
        {
            return;
        }

        _color = new Vector4(r, g, b, a);
    }

    /// <summary>glNormal3f.</summary>
    public void Normal(float x, float y, float z)
    {
        if (TryRecord(GlListOp.Normal, x, y, z))
        {
            return;
        }

        _normal = new Vector3(x, y, z);
    }

    /// <summary>glTexCoord2f.</summary>
    public void TexCoord(float s, float t)
    {
        if (TryRecord(GlListOp.TexCoord, s, t))
        {
            return;
        }

        _texCoord = new Vector2(s, t);
    }

    /// <summary>glClearColor.</summary>
    public void ClearColor(float r, float g, float b, float a)
    {
        _clearColor = new XnaColor(r, g, b, a);
    }

    /// <summary>glClear of color and depth, and of the stencil buffer (to 0) when <paramref name="includeStencil"/> is set.</summary>
    public void Clear(bool includeStencil = false)
    {
        Flush();
        var options = ClearOptions.Target | ClearOptions.DepthBuffer | (includeStencil ? ClearOptions.Stencil : 0);
        _device.Clear(options, _clearColor, 1f, 0);
    }

    /// <summary>glViewport, with OpenGL's bottom-left origin.</summary>
    public void Viewport(int x, int y, int width, int height)
    {
        Flush();
        var backBufferHeight = _device.PresentationParameters.BackBufferHeight;
        _device.Viewport = new Viewport(x, backBufferHeight - y - height, width, height);
    }

    // ---- Immediate mode -------------------------------------------------------------------------

    /// <summary>glBegin.</summary>
    public void Begin(GlPrimitive primitive)
    {
        if (TryRecord(GlListOp.Begin, (float)primitive, 0))
        {
            return;
        }

        _currentPrimitive = primitive;
        _primitive.Clear();
        _primitiveJobs.Clear();
        _primitiveBackCount = 0;
    }

    /// <summary>
    /// glVertex3f: transforms and stores the vertex with the current attributes. With lighting on, it is lit with
    /// the current lighting state when the batch is drawn.
    /// </summary>
    public void Vertex(float x, float y, float z)
    {
        if (TryRecord(GlListOp.Vertex, x, y, z))
        {
            return;
        }

        GlFrameCounters.CountVertex();
        var eyePosition = Vector3.Transform(new Vector3(x, y, z), _modelView);
        var material = Vector4.Clamp(_color, Vector4.Zero, Vector4.One);
        if (!_lighting)
        {
            _primitive.Add(new VertexPositionColorTexture(eyePosition, new XnaColor(material), _texCoord));
            _primitiveJobs.Add(Unlit);
            return;
        }

        // A two-sided vertex's back color is the job after its front color.
        var normal = EyeNormal();
        _primitiveJobs.Add(_shading.Add(eyePosition, normal, material, Lighting));
        if (Lighting.TwoSide)
        {
            _shading.Add(eyePosition, -normal, material, Lighting);
            _primitiveBackCount++;
        }

        _primitive.Add(new VertexPositionColorTexture(eyePosition, default, _texCoord));
    }

    /// <summary>glVertex2f.</summary>
    public void Vertex(float x, float y) => Vertex(x, y, 0f);

    /// <summary>glEnd: converts the primitive to triangles or lines and adds it to the batch.</summary>
    public void End()
    {
        if (TryRecord(GlListOp.End, 0, 0))
        {
            return;
        }

        if (_currentPrimitive is not { } primitive)
        {
            return;
        }

        _currentPrimitive = null;
        var isLine = primitive is GlPrimitive.Lines or GlPrimitive.LineStrip or GlPrimitive.LineLoop;
        if (isLine && IsWideLine)
        {
            EndWideLines(primitive);
            return;
        }

        if (primitive == GlPrimitive.Points)
        {
            return;
        }

        PrepareBatch(isLine, isLine);

        // The primitive's vertices are stored once and referenced by index; at most three indices per vertex (strips
        // and fans). Back-lit copies are added by the first back-facing triangle.
        var count = _primitive.Count;
        EnsureBatchSpace(2 * count, 3 * count);
        _primitiveBase = _batchCount;
        _primitiveBackBase = -1;
        for (var i = 0; i < count; i++)
        {
            AddBatchVertex(_primitive[i], _primitiveJobs[i]);
        }

        switch (primitive)
        {
            case GlPrimitive.Triangles:
                for (var i = 0; i + 2 < count; i += 3)
                {
                    AddTriangle(i, i + 1, i + 2);
                }

                break;
            case GlPrimitive.TriangleStrip:
                for (var i = 0; i + 2 < count; i++)
                {
                    if (i % 2 == 0)
                    {
                        AddTriangle(i, i + 1, i + 2);
                    }
                    else
                    {
                        AddTriangle(i + 1, i, i + 2);
                    }
                }

                break;
            case GlPrimitive.TriangleFan:
            case GlPrimitive.Polygon:
                for (var i = 1; i + 1 < count; i++)
                {
                    AddTriangle(0, i, i + 1);
                }

                break;
            case GlPrimitive.Quads:
                for (var i = 0; i + 3 < count; i += 4)
                {
                    AddTriangle(i, i + 1, i + 2);
                    AddTriangle(i, i + 2, i + 3);
                }

                break;
            case GlPrimitive.QuadStrip:
                for (var i = 0; i + 3 < count; i += 2)
                {
                    AddTriangle(i, i + 1, i + 3);
                    AddTriangle(i, i + 3, i + 2);
                }

                break;
            case GlPrimitive.Lines:
                for (var i = 0; i + 1 < count; i += 2)
                {
                    AddLine(i, i + 1);
                }

                break;
            case GlPrimitive.LineStrip:
            case GlPrimitive.LineLoop:
                for (var i = 0; i + 1 < count; i++)
                {
                    AddLine(i, i + 1);
                }

                if (primitive == GlPrimitive.LineLoop && count > 2)
                {
                    AddLine(count - 1, 0);
                }

                break;
            default:
                break;
        }
    }

    // ---- Display lists --------------------------------------------------------------------------

    /// <summary>glGenLists + glNewList(GL_COMPILE): starts recording calls into a new list.</summary>
    public GlDisplayList NewList()
    {
        _recording = new GlDisplayList();
        return _recording;
    }

    /// <summary>glEndList.</summary>
    public void EndList() => _recording = null;

    /// <summary>glCallList: replays the recorded calls.</summary>
    public void CallList(GlDisplayList list)
    {
        if (TryRecord(list, static (g, l) => g.CallList(l)))
        {
            return;
        }

        foreach (ref readonly var command in CollectionsMarshal.AsSpan(list.Commands))
        {
            switch (command.Op)
            {
                case GlListOp.Vertex:
                    Vertex(command.X, command.Y, command.Z);
                    break;
                case GlListOp.Normal:
                    _normal = new Vector3(command.X, command.Y, command.Z);
                    break;
                case GlListOp.TexCoord:
                    _texCoord = new Vector2(command.X, command.Y);
                    break;
                case GlListOp.Color:
                    _color = new Vector4(command.X, command.Y, command.Z, command.W);
                    break;
                case GlListOp.Begin:
                    Begin((GlPrimitive)(int)command.X);
                    break;
                case GlListOp.End:
                    End();
                    break;
                case GlListOp.Call:
                default:
                    command.Call!(this);
                    break;
            }
        }
    }

    // ---- Frame ----------------------------------------------------------------------------------

    /// <summary>
    /// Lights the waiting vertices and draws everything batched so far. Call at the end of a frame and before using
    /// the device directly.
    /// </summary>
    public void Flush()
    {
        CloseRange();
        if (_ranges.Count == 0)
        {
            return;
        }

        ApplyShading();
        UploadBatch();
        foreach (var range in _ranges)
        {
            Draw(range);
        }

        _device.SetVertexBuffer(null);
        _device.Indices = null;
        _ranges.Clear();
        _batchCount = 0;
        _indexCount = 0;
        _rangeStart = 0;
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        _effect.Dispose();
        _vertexBuffer?.Dispose();
        _indexBuffer?.Dispose();
        _depthTestState.Dispose();
        foreach (var state in _blendStates.Values)
        {
            state.Dispose();
        }

        DisposeFragmentStates();
    }

    private Matrix CurrentMatrix => _matrixMode == GlMatrixMode.ModelView ? _modelView : _projection;

    // Ends the current run of same-state vertices; it is drawn by the next Flush.
    private void CloseRange()
    {
        if (_indexCount > _rangeStart)
        {
            _ranges.Add(new GlDrawRange(_batchKey, _rangeStart, _indexCount - _rangeStart));
            _rangeStart = _indexCount;
        }
    }

    private void ApplyShading()
    {
        if (_shading.Count == 0)
        {
            return;
        }

        _shading.ShadeAll();
        if (_batchCount < ParallelColorThreshold)
        {
            ResolveColors(0, _batchCount);
        }
        else
        {
            Parallel.For(0, (_batchCount + ColorChunkSize - 1) / ColorChunkSize, _resolveColorChunk);
        }

        _shading.Clear();
    }

    private void ResolveColorChunk(int chunk) => ResolveColors(chunk * ColorChunkSize, Math.Min(_batchCount, (chunk + 1) * ColorChunkSize));

    private void ResolveColors(int start, int end)
    {
        for (var i = start; i < end; i++)
        {
            var job = _batchJobs[i];
            if (job != Unlit)
            {
                _batch[i].Color = _shading.ColorOf(job);
            }
        }
    }

    // One upload per flush; the ranges are then drawn from the vertex buffer.
    private void UploadBatch()
    {
        if (_vertexBuffer == null || _vertexBuffer.VertexCount < _batchCount)
        {
            _vertexBuffer?.Dispose();
            _vertexBuffer = new DynamicVertexBuffer(_device, VertexPositionColorTexture.VertexDeclaration, _batch.Length, BufferUsage.WriteOnly);
        }

        if (_indexBuffer == null || _indexBuffer.IndexCount < _indexCount)
        {
            _indexBuffer?.Dispose();
            _indexBuffer = new DynamicIndexBuffer(_device, IndexElementSize.ThirtyTwoBits, _indices.Length, BufferUsage.WriteOnly);
        }

        _vertexBuffer.SetData(_batch, 0, _batchCount, SetDataOptions.Discard);
        _indexBuffer.SetData(_indices, 0, _indexCount, SetDataOptions.Discard);
        _device.SetVertexBuffer(_vertexBuffer);
        _device.Indices = _indexBuffer;
    }

    private void Draw(GlDrawRange range)
    {
        var key = range.Key;
        _device.BlendState = key.Blend is { } blend ? GetBlendState(blend.Source, blend.Destination) : BlendState.Opaque;
        _device.DepthStencilState = GetDepthStencilState(key.DepthTest, key.Stencil);
        _device.RasterizerState = GetRasterizerState(key.Cull, key.PolygonOffset);
        _device.SamplerStates[0] = SamplerState.LinearWrap;
        _effect.Projection = key.Projection;
        _effect.TextureEnabled = key.Texture != null;
        _effect.Texture = key.Texture;
        GlFrameCounters.CountDraw();
        foreach (var pass in _effect.CurrentTechnique.Passes)
        {
            pass.Apply();
            if (key.IsLine)
            {
                _device.DrawIndexedPrimitives(PrimitiveType.LineList, 0, range.Start, range.Count / 2);
            }
            else
            {
                _device.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, range.Start, range.Count / 3);
            }
        }
    }

    private bool TryRecord(Action<LegacyGl> command)
    {
        if (_recording == null)
        {
            return false;
        }

        _recording.Add(command);
        return true;
    }

    // Arguments travel as state for a static lambda: a capturing lambda would allocate on every call, recording or not.
    private bool TryRecord<TState>(TState state, Action<LegacyGl, TState> command)
    {
        if (_recording == null)
        {
            return false;
        }

        _recording.Add(state, command);
        return true;
    }

    private bool TryRecord(GlListOp op, float x, float y, float z = 0, float w = 0)
    {
        if (_recording == null)
        {
            return false;
        }

        _recording.Add(op, x, y, z, w);
        return true;
    }

    // GL post-multiplies (M = M * T with column vectors), which is T * M with row vectors.
    private void MultiplyCurrent(Matrix transform) => SetCurrentMatrix(transform * CurrentMatrix);

    private void SetCurrentMatrix(Matrix matrix)
    {
        if (_matrixMode == GlMatrixMode.ModelView)
        {
            _modelView = matrix;
            _isNormalMatrixDirty = true;
        }
        else
        {
            _projection = matrix;
        }
    }

    private Vector3 EyeNormal()
    {
        if (_isNormalMatrixDirty)
        {
            _normalMatrix = Matrix.Transpose(Matrix.Invert(_modelView));
            _isNormalMatrixDirty = false;
        }

        var normal = Vector3.TransformNormal(_normal, _normalMatrix);
        if (!_normalize)
        {
            return normal;
        }

        var length = normal.Length();
        return length > 0 ? normal / length : Vector3.UnitZ;
    }

    private void SetCapability(GlCapability capability, bool enabled)
    {
        if (TryRecord((capability, enabled), static (g, a) => g.SetCapability(a.capability, a.enabled)))
        {
            return;
        }

        switch (capability)
        {
            case GlCapability.Blend:
                _blend = enabled;
                break;
            case GlCapability.DepthTest:
                _depthTest = enabled;
                break;
            case GlCapability.Lighting:
                _lighting = enabled;
                break;
            case GlCapability.Texture2D:
                _texture2D = enabled;
                break;
            case GlCapability.CullFace:
                _cullFace = enabled;
                break;
            case GlCapability.ColorMaterial:
                Lighting.ColorMaterial = enabled;
                break;
            case GlCapability.Light0:
            case GlCapability.Light1:
            case GlCapability.Light2:
                Lighting.Lights[capability - GlCapability.Light0].Enabled = enabled;
                break;
            case GlCapability.Normalize:
                _normalize = enabled;
                break;
            case GlCapability.StencilTest:
                _stencilTest = enabled;
                break;
            case GlCapability.PolygonOffsetFill:
                _polygonOffsetFill = enabled;
                break;
            case GlCapability.LineSmooth:
            default:
                break;
        }
    }

    // Starts a new batch when any state that affects drawing differs from the current batch. Line primitives are
    // never culled or offset, also when drawn as triangles.
    private void PrepareBatch(bool drawAsLines, bool isLinePrimitive)
    {
        var key = new GlBatchKey(
            _texture2D ? ResolveTexture(_boundTexture) : null,
            _blend ? (_sourceBlend, _destinationBlend) : null,
            _depthTest,
            !_cullFace || isLinePrimitive ? CullMode.None : _frontFaceClockwise ? CullMode.CullCounterClockwiseFace : CullMode.CullClockwiseFace,
            _stencilTest ? _stencil : null,
            _polygonOffsetFill && !isLinePrimitive ? _polygonOffset : null,
            _projection,
            drawAsLines);
        if (_indexCount > _rangeStart && !key.Equals(_batchKey))
        {
            CloseRange();
        }

        _batchKey = key;
    }

    // With two-sided lighting, back-facing triangles take the colors lit with the reversed normal. Space was ensured
    // by End.
    private void AddTriangle(int a, int b, int c)
    {
        var first = _primitiveBackCount == _primitive.Count && !IsFrontFacing(a, b, c) ? BackVertices() : _primitiveBase;
        _indices[_indexCount++] = first + a;
        _indices[_indexCount++] = first + b;
        _indices[_indexCount++] = first + c;
    }

    // The primitive's vertices again with the back colors, the jobs after the front ones.
    private int BackVertices()
    {
        if (_primitiveBackBase < 0)
        {
            _primitiveBackBase = _batchCount;
            for (var i = 0; i < _primitive.Count; i++)
            {
                AddBatchVertex(_primitive[i], _primitiveJobs[i] + 1);
            }
        }

        return _primitiveBackBase;
    }

    private bool IsFrontFacing(int a, int b, int c) => GlFacing.IsFrontFacing(
        Vector4.Transform(new Vector4(_primitive[a].Position, 1f), _projection),
        Vector4.Transform(new Vector4(_primitive[b].Position, 1f), _projection),
        Vector4.Transform(new Vector4(_primitive[c].Position, 1f), _projection),
        _frontFaceClockwise);

    private void AddLine(int a, int b)
    {
        _indices[_indexCount++] = _primitiveBase + a;
        _indices[_indexCount++] = _primitiveBase + b;
    }

    private void EnsureBatchSpace(int vertices, int indices)
    {
        if (_batchCount + vertices > _batch.Length)
        {
            var size = Math.Max(_batch.Length * 2, _batchCount + vertices);
            Array.Resize(ref _batch, size);
            Array.Resize(ref _batchJobs, size);
        }

        if (_indexCount + indices > _indices.Length)
        {
            Array.Resize(ref _indices, Math.Max(_indices.Length * 2, _indexCount + indices));
        }
    }

    private BlendState GetBlendState(GlBlend source, GlBlend destination)
    {
        if (!_blendStates.TryGetValue((source, destination), out var state))
        {
            state = new BlendState
            {
                ColorSourceBlend = ToXna(source),
                AlphaSourceBlend = ToXna(source),
                ColorDestinationBlend = ToXna(destination),
                AlphaDestinationBlend = ToXna(destination),
            };
            _blendStates[(source, destination)] = state;
        }

        return state;
    }

    private static Blend ToXna(GlBlend factor) => factor switch
    {
        GlBlend.Zero => Blend.Zero,
        GlBlend.One => Blend.One,
        GlBlend.SrcColor => Blend.SourceColor,
        GlBlend.OneMinusSrcColor => Blend.InverseSourceColor,
        GlBlend.SrcAlpha => Blend.SourceAlpha,
        GlBlend.OneMinusSrcAlpha => Blend.InverseSourceAlpha,
        GlBlend.DstColor => Blend.DestinationColor,
        GlBlend.OneMinusDstColor => Blend.InverseDestinationColor,
        GlBlend.DstAlpha => Blend.DestinationAlpha,
        GlBlend.OneMinusDstAlpha => Blend.InverseDestinationAlpha,
        _ => Blend.One,
    };
}
