using Saoss.Core.Ships;
using Saoss.Core.Simulation.Ai;

namespace Saoss.Core.Simulation;

/// <summary>
/// One player's ship and everything it owns: projectiles, slime, beams and tail history [Player].
/// Behavior lives in <see cref="ShipMovement"/>, <see cref="World"/> and the per-type weapon behaviors;
/// this class holds state and the respawn rules.
/// </summary>
public sealed class Ship
{
    /// <summary>Largest primary projectile count of any ship type; every ship has this many slots.</summary>
    public static readonly int PrimaryPoolSize = ShipCatalog.All.Max(d => d.Primary.MaxProjectiles);

    /// <summary>Largest special projectile count of any ship type.</summary>
    public static readonly int SpecialPoolSize = ShipCatalog.All.Max(d => d.Special.MaxProjectiles);

    private const int RespawnFireCooldown = 40;
    private const int RespawnSpecialCooldown = 20;
    private const float MinRespawnDistanceFromCenter = GameConstants.PlanetRadius * 10f;
    private const int SlimeLifetime = 400;
    private const float SlimeSpreadRoll = 1500f;
    private const float SlimeSpreadDivisor = 1000f;
    private const float SlimeRadiusScale = 0.785f;
    private const float SlimeOffset = 0.75f;

    /// <summary>Creates an empty slot; call <see cref="Respawn"/> before the ship is used.</summary>
    public Ship(int index, int team, bool isHuman)
    {
        Index = index;
        Team = team;
        IsHuman = isHuman;
        Definition = ShipCatalog.All[0];
        PrimaryProjectiles = CreatePool(PrimaryPoolSize);
        SpecialProjectiles = CreatePool(SpecialPoolSize);
        Slimes = CreatePool(GameConstants.MaxSlimes);
        Beams = [.. Enumerable.Range(0, GameConstants.MaxBeams).Select(_ => new Beam())];
    }

    /// <summary>Player slot number [numero].</summary>
    public int Index { get; }

    /// <summary>Team number; ships never hurt their own team with area weapons [tiimi].</summary>
    public int Team { get; set; }

    /// <summary>Controlled by a person rather than the AI [ihmispelaaja].</summary>
    public bool IsHuman { get; set; }

    /// <summary>False once the player is out of a credits game (original: negative score).</summary>
    public bool IsInGame { get; set; } = true;

    /// <summary>Score or remaining credits [pisteet].</summary>
    public float Score { get; set; }

    /// <summary>Current ship type and its stats [alus].</summary>
    public ShipDefinition Definition { get; private set; }

    /// <summary>Current ship type.</summary>
    public ShipType Type => Definition.Type;

    /// <summary>Buttons held this tick.</summary>
    public ShipControls Controls { get; set; }

    /// <summary>Position in world units.</summary>
    public float X { get; set; }

    /// <inheritdoc cref="X"/>
    public float Y { get; set; }

    /// <summary>Velocity per tick [xvauhti, yvauhti].</summary>
    public float VelocityX { get; set; }

    /// <inheritdoc cref="VelocityX"/>
    public float VelocityY { get; set; }

    /// <summary>Heading in degrees, kept in [-180, 180] [kulma].</summary>
    public float Angle { get; set; }

    /// <summary>Gun direction in degrees; lags behind <see cref="Angle"/> [asekulma].</summary>
    public float WeaponAngle { get; set; }

    /// <summary>Hit points; the ship explodes at zero or below [energia].</summary>
    public float Energy { get; set; }

    /// <summary>Fuel for shooting and specials [fuel].</summary>
    public float Fuel { get; set; }

    /// <summary>Ticks until the primary weapon can fire again [ampumisviive].</summary>
    public int FireCooldown { get; set; }

    /// <summary>Ticks until the special can be used again [easeviive].</summary>
    public int SpecialCooldown { get; set; }

    /// <summary>Ticks the current special still lasts [easekesto].</summary>
    public int SpecialTimeLeft { get; set; }

    /// <summary>Shield or other special mode is on [suojapaalla].</summary>
    public bool IsShieldActive { get; set; }

    /// <summary>
    /// Zero while alive. Set to <see cref="GameConstants.DeathTicks"/> + <see cref="GameConstants.SpawnTicks"/>
    /// on explosion and counted down; the new ship is placed when it reaches <see cref="GameConstants.SpawnTicks"/> [kuolema].
    /// </summary>
    public int DeathTimer { get; set; }

    /// <summary>Weapon-specific state, e.g. which side fires next [ampumispuoli].</summary>
    public int FireSide { get; set; }

    /// <summary>A teleport is pending to <see cref="TeleportX"/>, <see cref="TeleportY"/> [teleportattu].</summary>
    public bool IsTeleportPending { get; set; }

    /// <summary>Teleport target [uusx, uusy].</summary>
    public float TeleportX { get; set; }

    /// <inheritdoc cref="TeleportX"/>
    public float TeleportY { get; set; }

    /// <summary>Propeller rotation in degrees for ships that have one [propelli].</summary>
    public float PropellerAngle { get; set; }

    /// <summary>Weapon-specific charge or glow value that decays each tick [ampumisjuttu].</summary>
    public float WeaponCharge { get; set; }

    /// <summary>Weapon-specific tick counter [fcounter].</summary>
    public float WeaponCounter { get; set; }

    /// <summary>A looping fire sound is playing and must be stopped when fire is released [laseraani].</summary>
    public bool IsFireLoopPlaying { get; set; }

    /// <summary>Whether the engine was firing during the last tick; used for exhaust effects.</summary>
    public bool IsThrusting { get; set; }

    /// <summary>Number of slime blobs stuck on the ship; each slows it down [limacount].</summary>
    public int SlimeCount { get; set; }

    /// <summary>Memory of a handicapped computer pilot; unused by humans and the original pilot.</summary>
    public AiState Ai { get; } = new();

    /// <summary>Primary weapon projectile pool; only the first <c>Primary.MaxProjectiles</c> slots are used [pati].</summary>
    public Projectile[] PrimaryProjectiles { get; }

    /// <summary>Special weapon projectile pool; only the first <c>Special.MaxProjectiles</c> slots are used [epati].</summary>
    public Projectile[] SpecialProjectiles { get; }

    /// <summary>Slime blobs stuck on the ship, in ship-local coordinates [lima].</summary>
    public Projectile[] Slimes { get; }

    /// <summary>Lines drawn by weapons [viivat].</summary>
    public Beam[] Beams { get; }

    /// <summary>Recent positions and headings, newest first [xhanta, yhanta, kulmahanta].</summary>
    public float[] TailX { get; } = new float[GameConstants.TailLength];

    /// <inheritdoc cref="TailX"/>
    public float[] TailY { get; } = new float[GameConstants.TailLength];

    /// <inheritdoc cref="TailX"/>
    public float[] TailAngle { get; } = new float[GameConstants.TailLength];

    /// <summary>The ship is flying (not exploded or respawning).</summary>
    public bool IsAlive => DeathTimer <= 0;

    /// <summary>Velocity multiplier from stuck slime, 1 when clean.</summary>
    public float SlimeSlowdown => 1f - ((float)SlimeCount / GameConstants.MaxSlimes * 0.8f);

    /// <summary>Unit vector of the heading.</summary>
    public (float X, float Y) Heading
    {
        get
        {
            var radians = Angle / 180f * MathF.PI;
            return (MathF.Cos(radians), MathF.Sin(radians));
        }
    }

    /// <summary>
    /// Gives the ship a new type at a random position away from the planet, with full energy and fuel
    /// and all weapons cleared [uudestaan]. Heading is kept.
    /// </summary>
    public void Respawn(ShipDefinition definition, GameRandom random)
    {
        Definition = definition;
        FireCooldown = RespawnFireCooldown;
        SpecialCooldown = RespawnSpecialCooldown;
        X = 0;
        Y = 0;
        while (MathF.Sqrt((X * X) + (Y * Y)) < MinRespawnDistanceFromCenter)
        {
            X = random.Range(GameConstants.ArenaMinX, GameConstants.ArenaMaxX);
            Y = random.Range(GameConstants.ArenaMinY, GameConstants.ArenaMaxY);
        }

        VelocityX = 0;
        VelocityY = 0;
        Energy = definition.MaxEnergy;
        Fuel = definition.MaxFuel;
        ClearPool(PrimaryProjectiles);
        ClearPool(SpecialProjectiles);
        ClearPool(Slimes);
        SlimeCount = 0;
        foreach (var beam in Beams)
        {
            beam.Lifetime = 0;
            beam.X1 = 100;
        }

        FireSide = 1;
        IsTeleportPending = false;
        IsShieldActive = false;
        WeaponCharge = 0;
        WeaponCounter = 0;
        ResetTail();
    }

    /// <summary>
    /// Sticks one slime blob at a random spot on the ship, unless it is already fully covered [lisaalima].
    /// Slime never wears off; only a respawn clears it.
    /// </summary>
    public void AddSlime(GameRandom random)
    {
        if (SlimeCount >= GameConstants.MaxSlimes)
        {
            return;
        }

        var slime = Slimes[SlimeCount];
        slime.Lifetime = SlimeLifetime;
        slime.X = (random.Next(SlimeSpreadRoll) / SlimeSpreadDivisor * Definition.Radius * SlimeRadiusScale) - SlimeOffset;
        slime.Y = (random.Next(SlimeSpreadRoll) / SlimeSpreadDivisor * Definition.Radius * SlimeRadiusScale) - SlimeOffset;
        SlimeCount++;
    }

    /// <summary>Records the current position as the newest tail entry after shifting older ones back.</summary>
    public void ShiftTail(bool recordCurrent)
    {
        for (var i = GameConstants.TailLength - 1; i > 0; i--)
        {
            TailX[i] = TailX[i - 1];
            TailY[i] = TailY[i - 1];
            TailAngle[i] = TailAngle[i - 1];
        }

        if (recordCurrent)
        {
            TailX[0] = X;
            TailY[0] = Y;
            TailAngle[0] = Angle;
        }
    }

    /// <summary>Sets the ship type without any of the respawn side effects; used when restoring a snapshot.</summary>
    internal void RestoreDefinition(ShipDefinition definition) => Definition = definition;

    private static Projectile[] CreatePool(int size) => [.. Enumerable.Range(0, size).Select(_ => new Projectile())];

    private static void ClearPool(Projectile[] pool)
    {
        foreach (var projectile in pool)
        {
            projectile.Lifetime = 0;
        }
    }

    private void ResetTail()
    {
        for (var i = 0; i < GameConstants.TailLength; i++)
        {
            TailX[i] = X - (Definition.Thrust * i);
            TailY[i] = Y;
            TailAngle[i] = Angle;
        }
    }
}
