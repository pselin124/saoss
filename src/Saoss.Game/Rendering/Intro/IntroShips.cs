using Saoss.Core;
using Saoss.Core.Ships;
using Saoss.Core.Simulation;
using Saoss.Game.Rendering.Legacy;
using Saoss.Game.Rendering.Ships;

namespace Saoss.Game.Rendering.Intro;

/// <summary>
/// The ships of the intro (the "Vieraan jutun alku" block of lippu.cpp <c>draw</c>): a Stick Insect standing on the
/// meadow, a Rayska circling the pole, a Moth fluttering past, a Bulldog tumbling across the sky from a random start,
/// and a random ship crossing the meadow. Each repeats on its own frame cycle.
/// </summary>
public sealed class IntroShips
{
    // The original forced detail 1 while drawing the intro's ships.
    private const int ShipDetail = 1;
    private const int RayskaCycle = 2400;
    private const int BulldogCycle = 2700;
    private const float BulldogDelay = 400f;
    private const int BulldogHiddenUntil = -350;
    private const int PasserCycle = 1500;
    private const int PasserPickFrames = 30;
    private const int MothFirstFrame = 250;
    private const int MothLastFrame = 750;
    private const float DegreesPerRadianApprox = 180f / 3.14f;

    // random(15) picks one of these; the original's switch has no case 7, which falls to the default (Popper).
    private static readonly ShipType[] PasserTypes =
    [
        ShipType.Tringu, ShipType.Wasp, ShipType.Ufo, ShipType.Rayska, ShipType.PeaceCannon, ShipType.Shapperi,
        ShipType.Moth, ShipType.Popper, ShipType.Bulldog, ShipType.Wiiii, ShipType.Cubix, ShipType.Tongu,
        ShipType.StickInsect, ShipType.StarWorm,
    ];

    private readonly LegacyGl _gl;
    private readonly ShipModelRenderer _renderer;
    private readonly Random _random;
    private readonly Dictionary<ShipType, Ship> _ships = [];
    private float _bulldogStartX;
    private float _bulldogStartY;
    private float _bulldogStartZ;
    private ShipType _passer = ShipType.Magneto;

    /// <summary>Creates one idle ship of every type to draw from.</summary>
    public IntroShips(LegacyGl gl, ShipModelRenderer renderer, Random random)
    {
        _gl = gl;
        _renderer = renderer;
        _random = random;
        var shipRandom = new GameRandom(1);
        foreach (var type in Enum.GetValues<ShipType>())
        {
            var ship = new Ship((int)type, team: 0, isHuman: false);
            ship.Respawn(ShipCatalog.Get(type), shipRandom);
            ship.Angle = 0;
            ship.WeaponAngle = 0;
            _ships[type] = ship;
        }
    }

    /// <summary>
    /// Shows or hides the exhaust parts of every intro ship. The original drew them all with player 1's state, whose
    /// thrust followed the Up arrow even during the intro.
    /// </summary>
    public void SetThrust(bool thrust)
    {
        foreach (var ship in _ships.Values)
        {
            ship.Controls = ship.Controls with { Thrust = thrust };
        }
    }

    /// <summary>Draws all ships of intro frame <paramref name="frame"/> in the meadow's unscaled frame.</summary>
    public void Draw(int frame)
    {
        var rayskaFrame = frame % RayskaCycle;
        var bulldogFrame = (int)((frame % BulldogCycle) - BulldogDelay);
        var passerFrame = frame % PasserCycle;

        DrawStickInsect();
        DrawRayska(rayskaFrame);
        if (rayskaFrame > MothFirstFrame && rayskaFrame < MothLastFrame)
        {
            DrawMoth(rayskaFrame);
        }

        DrawBulldog(bulldogFrame);
        DrawPasser(passerFrame);
    }

    /// <summary>Draws the standing Stick Insect's shadow shape (a little off its feet, as in the original); the caller sets up the shadow state.</summary>
    public void DrawStickInsectShadow()
    {
        _gl.PushMatrix();
        _gl.Translate(8.5f, 0.0f, -5);
        IntroScene.FlattenOntoGround(_gl);
        _gl.Rotate(90, 0, 0, 1);
        _gl.Rotate(-45, 1, 0, 0);
        _renderer.DrawModelOnly(_ships[ShipType.StickInsect], ShipDetail, shadow: true);
        _gl.PopMatrix();
    }

    private void DrawStickInsect()
    {
        _gl.PushMatrix();
        _gl.Translate(10, 2.2f, -5);
        _gl.Rotate(90, 0, 0, 1);
        _gl.Rotate(-45, 1, 0, 0);
        _renderer.DrawModelOnly(_ships[ShipType.StickInsect], ShipDetail);
        _gl.PopMatrix();
    }

    // An ellipse around the pole, 12 units up, banking with its heading.
    private void DrawRayska(int rayskaFrame)
    {
        var phase = rayskaFrame / (RayskaCycle / (2f * MathF.PI));
        _gl.PushMatrix();
        _gl.Rotate(180, 0, 1, 0);
        _gl.Translate(MathF.Cos(phase) * 20f, 12, MathF.Sin(phase) * 25f);
        _gl.Rotate(-90, 1, 0, 0);
        _gl.Rotate((MathF.Cos(phase) * DegreesPerRadianApprox) - 180f, 0, 0, 1);
        _gl.Rotate(20, 1, 0, 0);
        _gl.Scale(1.5f, 1.5f, 1.5f);
        _renderer.DrawModelOnly(_ships[ShipType.Rayska], ShipDetail);
        _gl.PopMatrix();
    }

    private void DrawMoth(int rayskaFrame)
    {
        _gl.PushMatrix();
        _gl.Rotate(180, 0, 1, 0);
        _gl.Translate((MathF.Sin(rayskaFrame / 100.0f) * 35.0f) - 20, (MathF.Cos(rayskaFrame / 300.0f) * 40) + 20, 34);
        _gl.Rotate(-MathF.Cos(rayskaFrame / 400.0f) * DegreesPerRadianApprox, 0, 0, 1);
        _gl.Rotate(-20, 1, 0, 0);
        _gl.Scale(1.5f, 1.5f, 1.5f);
        _renderer.DrawModelOnly(_ships[ShipType.Moth], ShipDetail);
        _gl.PopMatrix();
    }

    // Hidden at the start of each cycle, where a new random start point is chosen every drawn frame.
    private void DrawBulldog(int bulldogFrame)
    {
        if (bulldogFrame < BulldogHiddenUntil)
        {
            _bulldogStartX = Random(10) + 95.0f;
            _bulldogStartY = Random(10) - 32.0f;
            _bulldogStartZ = (Random(150) / 5.0f) - 15.0f;
            return;
        }

        _gl.PushMatrix();
        _gl.Rotate(90, 0, 1, 0);
        _gl.Translate(_bulldogStartX - (bulldogFrame * 0.1f), _bulldogStartY + (bulldogFrame * 0.05f), _bulldogStartZ);
        _gl.Rotate(180, 0, 1, 0);
        _gl.Rotate(bulldogFrame / 1.6f, 1, 0, 0);
        _renderer.DrawModelOnly(_ships[ShipType.Bulldog], ShipDetail);
        _gl.PopMatrix();
    }

    // The first frames of each cycle pick the ship; the rest fly it across the meadow toward -X.
    private void DrawPasser(int passerFrame)
    {
        if (passerFrame < PasserPickFrames)
        {
            var pick = (int)Random(PasserTypes.Length + 1);
            _passer = pick < PasserTypes.Length ? PasserTypes[pick] : ShipType.Popper;
            return;
        }

        _gl.PushMatrix();
        _gl.Translate(75 - (passerFrame * 0.1f), 6, 8);
        _gl.Rotate(180, 0, 1, 0);
        _gl.Scale(0.6f, 0.6f, 0.6f);
        _gl.Rotate(-90, 1, 0, 0);
        _renderer.DrawModelOnly(_ships[_passer], ShipDetail);
        _gl.PopMatrix();
    }

    // random(x) of Alustuksia.cpp: uniform in [0, x].
    private float Random(float max) => (float)(_random.NextDouble() * max);
}
