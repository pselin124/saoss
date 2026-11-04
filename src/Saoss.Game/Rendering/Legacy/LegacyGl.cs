using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using XnaColor = Microsoft.Xna.Framework.Color;

namespace Saoss.Game.Rendering.Legacy;

/// <summary>
/// A small OpenGL 1.x fixed-function emulation on top of MonoGame, so the original drawing code can be
/// translated call by call. Vertices are transformed to eye space and lit on the CPU (including point
/// lights), then batched by render state and drawn with a <see cref="BasicEffect"/>.
/// Method names follow GL without the <c>gl</c> prefix; angles are degrees. Unlike OpenGL, GL_NORMALIZE starts
/// enabled, the state the game's own setup left for everything but the intro.
/// </summary>
public sealed partial class LegacyGl : IDisposable
{
    private readonly GraphicsDevice _device;
    private readonly BasicEffect _effect;
    private readonly Stack<Matrix> _modelViewStack = new();
    private readonly Stack<Matrix> _projectionStack = new();
    private readonly List<VertexPositionColorTexture> _primitive = [];
    private readonly List<XnaColor> _primitiveBackColors = [];
    private readonly Dictionary<(GlBlend, GlBlend), BlendState> _blendStates = [];
    private readonly DepthStencilState _depthTestState = new() { DepthBufferEnable = true, DepthBufferWriteEnable = true, DepthBufferFunction = CompareFunction.LessEqual };

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

    private VertexPositionColorTexture[] _batch = new VertexPositionColorTexture[4096];
    private int _batchCount;
    private GlBatchKey _batchKey;

    private GlDisplayList? _recording;

    /// <summary>Creates the emulation for the given device.</summary>
    public LegacyGl(GraphicsDevice device)
    {
        _device = device;
        _effect = new BasicEffect(device) { LightingEnabled = false, VertexColorEnabled = true, World = Matrix.Identity, View = Matrix.Identity };
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
        if (TryRecord(g => g.MatrixMode(mode)))
        {
            return;
        }

        _matrixMode = mode;
    }

    /// <summary>glLoadIdentity.</summary>
    public void LoadIdentity()
    {
        if (TryRecord(g => g.LoadIdentity()))
        {
            return;
        }

        SetCurrentMatrix(Matrix.Identity);
    }

    /// <summary>glPushMatrix.</summary>
    public void PushMatrix()
    {
        if (TryRecord(g => g.PushMatrix()))
        {
            return;
        }

        (_matrixMode == GlMatrixMode.ModelView ? _modelViewStack : _projectionStack).Push(CurrentMatrix);
    }

    /// <summary>glPopMatrix.</summary>
    public void PopMatrix()
    {
        if (TryRecord(g => g.PopMatrix()))
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
        if (TryRecord(g => g.Translate(x, y, z)))
        {
            return;
        }

        MultiplyCurrent(Matrix.CreateTranslation(x, y, z));
    }

    /// <summary>glRotatef: counter-clockwise degrees around the axis.</summary>
    public void Rotate(float degrees, float x, float y, float z)
    {
        if (TryRecord(g => g.Rotate(degrees, x, y, z)))
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
        if (TryRecord(g => g.Scale(x, y, z)))
        {
            return;
        }

        MultiplyCurrent(Matrix.CreateScale(x, y, z));
    }

    /// <summary>gluPerspective, without argument validation (the original sometimes passes odd planes).</summary>
    public void Perspective(float fovYDegrees, float aspect, float near, float far)
    {
        if (TryRecord(g => g.Perspective(fovYDegrees, aspect, near, far)))
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
        if (TryRecord(g => g.Ortho(left, right, bottom, top, near, far)))
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
        if (TryRecord(g => g.LookAt(eyeX, eyeY, eyeZ, centerX, centerY, centerZ, upX, upY, upZ)))
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
        if (TryRecord(g => g.BlendFunc(source, destination)))
        {
            return;
        }

        _sourceBlend = source;
        _destinationBlend = destination;
    }

    /// <summary>glFrontFace: true for GL_CW.</summary>
    public void FrontFaceClockwise(bool clockwise)
    {
        if (TryRecord(g => g.FrontFaceClockwise(clockwise)))
        {
            return;
        }

        _frontFaceClockwise = clockwise;
    }

    /// <summary>glBindTexture; null unbinds (texturing then has no effect).</summary>
    public void BindTexture(Texture2D? texture)
    {
        if (TryRecord(g => g.BindTexture(texture)))
        {
            return;
        }

        _boundTexture = texture;
    }

    /// <summary>glColor4f.</summary>
    public void Color(float r, float g, float b, float a = 1f)
    {
        if (TryRecord(gl => gl.Color(r, g, b, a)))
        {
            return;
        }

        _color = new Vector4(r, g, b, a);
    }

    /// <summary>glNormal3f.</summary>
    public void Normal(float x, float y, float z)
    {
        if (TryRecord(g => g.Normal(x, y, z)))
        {
            return;
        }

        _normal = new Vector3(x, y, z);
    }

    /// <summary>glTexCoord2f.</summary>
    public void TexCoord(float s, float t)
    {
        if (TryRecord(g => g.TexCoord(s, t)))
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
        if (TryRecord(g => g.Begin(primitive)))
        {
            return;
        }

        _currentPrimitive = primitive;
        _primitive.Clear();
        _primitiveBackColors.Clear();
    }

    /// <summary>glVertex3f: transforms, lights and stores the vertex with the current attributes.</summary>
    public void Vertex(float x, float y, float z)
    {
        if (TryRecord(g => g.Vertex(x, y, z)))
        {
            return;
        }

        var eyePosition = Vector3.Transform(new Vector3(x, y, z), _modelView);
        var color = _color;
        if (_lighting)
        {
            var normal = EyeNormal();
            var material = Vector4.Clamp(_color, Vector4.Zero, Vector4.One);
            color = Lighting.Shade(eyePosition, normal, material);
            if (Lighting.TwoSide)
            {
                _primitiveBackColors.Add(new XnaColor(Lighting.Shade(eyePosition, -normal, material)));
            }
        }

        _primitive.Add(new VertexPositionColorTexture(eyePosition, new XnaColor(Vector4.Clamp(color, Vector4.Zero, Vector4.One)), _texCoord));
    }

    /// <summary>glVertex2f.</summary>
    public void Vertex(float x, float y) => Vertex(x, y, 0f);

    /// <summary>glEnd: converts the primitive to triangles or lines and adds it to the batch.</summary>
    public void End()
    {
        if (TryRecord(g => g.End()))
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

        PrepareBatch(isLine, isLine);
        switch (primitive)
        {
            case GlPrimitive.Triangles:
                for (var i = 0; i + 2 < _primitive.Count; i += 3)
                {
                    AddTriangle(i, i + 1, i + 2);
                }

                break;
            case GlPrimitive.TriangleStrip:
                for (var i = 0; i + 2 < _primitive.Count; i++)
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
                for (var i = 1; i + 1 < _primitive.Count; i++)
                {
                    AddTriangle(0, i, i + 1);
                }

                break;
            case GlPrimitive.Quads:
                for (var i = 0; i + 3 < _primitive.Count; i += 4)
                {
                    AddTriangle(i, i + 1, i + 2);
                    AddTriangle(i, i + 2, i + 3);
                }

                break;
            case GlPrimitive.QuadStrip:
                for (var i = 0; i + 3 < _primitive.Count; i += 2)
                {
                    AddTriangle(i, i + 1, i + 3);
                    AddTriangle(i, i + 3, i + 2);
                }

                break;
            case GlPrimitive.Lines:
                for (var i = 0; i + 1 < _primitive.Count; i += 2)
                {
                    AddLine(i, i + 1);
                }

                break;
            case GlPrimitive.LineStrip:
            case GlPrimitive.LineLoop:
                for (var i = 0; i + 1 < _primitive.Count; i++)
                {
                    AddLine(i, i + 1);
                }

                if (primitive == GlPrimitive.LineLoop && _primitive.Count > 2)
                {
                    AddLine(_primitive.Count - 1, 0);
                }

                break;
            case GlPrimitive.Points:
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
        if (TryRecord(g => g.CallList(list)))
        {
            return;
        }

        foreach (var command in list.Commands)
        {
            command(this);
        }
    }

    // ---- Frame ----------------------------------------------------------------------------------

    /// <summary>Draws everything batched so far. Call at the end of a frame and before using the device directly.</summary>
    public void Flush()
    {
        if (_batchCount == 0)
        {
            return;
        }

        var key = _batchKey;
        _device.BlendState = key.Blend is { } blend ? GetBlendState(blend.Source, blend.Destination) : BlendState.Opaque;
        _device.DepthStencilState = GetDepthStencilState(key.DepthTest, key.Stencil);
        _device.RasterizerState = GetRasterizerState(key.Cull, key.PolygonOffset);
        _device.SamplerStates[0] = SamplerState.LinearWrap;
        _effect.Projection = key.Projection;
        _effect.TextureEnabled = key.Texture != null;
        _effect.Texture = key.Texture;
        foreach (var pass in _effect.CurrentTechnique.Passes)
        {
            pass.Apply();
            if (key.IsLine)
            {
                _device.DrawUserPrimitives(PrimitiveType.LineList, _batch, 0, _batchCount / 2);
            }
            else
            {
                _device.DrawUserPrimitives(PrimitiveType.TriangleList, _batch, 0, _batchCount / 3);
            }
        }

        _batchCount = 0;
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        _effect.Dispose();
        _depthTestState.Dispose();
        foreach (var state in _blendStates.Values)
        {
            state.Dispose();
        }

        DisposeFragmentStates();
    }

    private Matrix CurrentMatrix => _matrixMode == GlMatrixMode.ModelView ? _modelView : _projection;

    private bool TryRecord(Action<LegacyGl> command)
    {
        if (_recording == null)
        {
            return false;
        }

        _recording.Commands.Add(command);
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
        if (TryRecord(g => g.SetCapability(capability, enabled)))
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
        if (_batchCount > 0 && !key.Equals(_batchKey))
        {
            Flush();
        }

        _batchKey = key;
    }

    // With two-sided lighting, back-facing triangles take the colors lit with the reversed normal.
    private void AddTriangle(int a, int b, int c)
    {
        EnsureBatchSpace(3);
        if (_primitiveBackColors.Count == _primitive.Count && !IsFrontFacing(a, b, c))
        {
            _batch[_batchCount++] = WithColor(_primitive[a], _primitiveBackColors[a]);
            _batch[_batchCount++] = WithColor(_primitive[b], _primitiveBackColors[b]);
            _batch[_batchCount++] = WithColor(_primitive[c], _primitiveBackColors[c]);
            return;
        }

        _batch[_batchCount++] = _primitive[a];
        _batch[_batchCount++] = _primitive[b];
        _batch[_batchCount++] = _primitive[c];
    }

    private static VertexPositionColorTexture WithColor(VertexPositionColorTexture vertex, XnaColor color) =>
        new(vertex.Position, color, vertex.TextureCoordinate);

    private bool IsFrontFacing(int a, int b, int c) => GlFacing.IsFrontFacing(
        Vector4.Transform(new Vector4(_primitive[a].Position, 1f), _projection),
        Vector4.Transform(new Vector4(_primitive[b].Position, 1f), _projection),
        Vector4.Transform(new Vector4(_primitive[c].Position, 1f), _projection),
        _frontFaceClockwise);

    private void AddLine(int a, int b)
    {
        EnsureBatchSpace(2);
        _batch[_batchCount++] = _primitive[a];
        _batch[_batchCount++] = _primitive[b];
    }

    private void EnsureBatchSpace(int count)
    {
        if (_batchCount + count > _batch.Length)
        {
            Array.Resize(ref _batch, _batch.Length * 2);
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
