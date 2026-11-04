using Saoss.Core.Ships;
using Saoss.Core.Simulation.Ai;

namespace Saoss.Core.Simulation.Snapshots;

/// <summary>
/// Writes and reads everything a <see cref="Ship"/> holds: type, controls, physics, weapon state, tail history, its
/// pools and the computer pilot's memory.
/// </summary>
internal static class ShipSnapshot
{
    private const int IsHumanFlag = 1;
    private const int IsInGameFlag = 2;
    private const int IsShieldActiveFlag = 4;
    private const int IsTeleportPendingFlag = 8;
    private const int IsFireLoopPlayingFlag = 16;
    private const int IsThrustingFlag = 32;
    private const int AllFlags = 63;

    /// <summary>Writes one ship.</summary>
    public static void Write(BinaryWriter writer, Ship ship)
    {
        writer.Write((byte)ship.Type);
        writer.Write((byte)ship.Team);
        writer.Write((byte)Flags(ship));
        writer.Write(ShipControlsBits.ToBits(ship.Controls));
        PoolSnapshot.WriteFloats(
            writer,
            ship.Score, ship.X, ship.Y, ship.VelocityX, ship.VelocityY, ship.Angle, ship.WeaponAngle, ship.Energy, ship.Fuel,
            ship.TeleportX, ship.TeleportY, ship.PropellerAngle, ship.WeaponCharge, ship.WeaponCounter);
        writer.Write(ship.FireCooldown);
        writer.Write(ship.SpecialCooldown);
        writer.Write(ship.SpecialTimeLeft);
        writer.Write(ship.DeathTimer);
        writer.Write(ship.FireSide);
        writer.Write((byte)ship.SlimeCount);
        PoolSnapshot.WriteFloats(writer, ship.TailX);
        PoolSnapshot.WriteFloats(writer, ship.TailY);
        PoolSnapshot.WriteFloats(writer, ship.TailAngle);
        PoolSnapshot.WriteProjectiles(writer, ship.PrimaryProjectiles);
        PoolSnapshot.WriteProjectiles(writer, ship.SpecialProjectiles);
        PoolSnapshot.WriteProjectiles(writer, ship.Slimes);
        PoolSnapshot.WriteBeams(writer, ship.Beams);
        WriteAi(writer, ship.Ai);
    }

    /// <summary>Reads one ship written by <see cref="Write"/>.</summary>
    public static void Read(SnapshotReader reader, Ship ship)
    {
        ship.RestoreDefinition(ShipCatalog.Get(reader.ReadEnum<ShipType>("ship type")));
        ship.Team = reader.ReadByte(GameConstants.MaxPlayers - 1, "team");
        ApplyFlags(ship, reader.ReadByte(AllFlags, "ship flags"));
        ship.Controls = ShipControlsBits.FromBits((byte)reader.ReadByte(ShipControlsBits.AllBits, "controls"));
        ReadPhysics(reader, ship);
        ship.FireCooldown = reader.ReadInt();
        ship.SpecialCooldown = reader.ReadInt();
        ship.SpecialTimeLeft = reader.ReadInt();
        ship.DeathTimer = reader.ReadInt();
        ship.FireSide = reader.ReadInt();
        ship.SlimeCount = reader.ReadByte(GameConstants.MaxSlimes, "slime count");
        ReadFloats(reader, ship.TailX);
        ReadFloats(reader, ship.TailY);
        ReadFloats(reader, ship.TailAngle);
        PoolSnapshot.ReadProjectiles(reader, ship.PrimaryProjectiles, "primary projectile");
        PoolSnapshot.ReadProjectiles(reader, ship.SpecialProjectiles, "special projectile");
        PoolSnapshot.ReadProjectiles(reader, ship.Slimes, "slime");
        PoolSnapshot.ReadBeams(reader, ship.Beams);
        ReadAi(reader, ship.Ai);
    }

    private static void WriteAi(BinaryWriter writer, AiState ai)
    {
        PoolSnapshot.WriteFloats(writer, ai.DistanceScale, ai.AimErrorDegrees, ai.TargetVelocityX, ai.TargetVelocityY);
        writer.Write(ai.DistanceTicksLeft);
        writer.Write(ai.AimTicksLeft);
        writer.Write(ai.MoodTicksLeft);
        writer.Write(ai.IsHoldingBack);
    }

    private static void ReadAi(SnapshotReader reader, AiState ai)
    {
        ai.DistanceScale = reader.ReadFloat();
        ai.AimErrorDegrees = reader.ReadFloat();
        ai.TargetVelocityX = reader.ReadFloat();
        ai.TargetVelocityY = reader.ReadFloat();
        ai.DistanceTicksLeft = reader.ReadInt();
        ai.AimTicksLeft = reader.ReadInt();
        ai.MoodTicksLeft = reader.ReadInt();
        ai.IsHoldingBack = reader.ReadBool("AI mood");
    }

    private static int Flags(Ship ship) =>
        (ship.IsHuman ? IsHumanFlag : 0)
        | (ship.IsInGame ? IsInGameFlag : 0)
        | (ship.IsShieldActive ? IsShieldActiveFlag : 0)
        | (ship.IsTeleportPending ? IsTeleportPendingFlag : 0)
        | (ship.IsFireLoopPlaying ? IsFireLoopPlayingFlag : 0)
        | (ship.IsThrusting ? IsThrustingFlag : 0);

    private static void ApplyFlags(Ship ship, int flags)
    {
        ship.IsHuman = (flags & IsHumanFlag) != 0;
        ship.IsInGame = (flags & IsInGameFlag) != 0;
        ship.IsShieldActive = (flags & IsShieldActiveFlag) != 0;
        ship.IsTeleportPending = (flags & IsTeleportPendingFlag) != 0;
        ship.IsFireLoopPlaying = (flags & IsFireLoopPlayingFlag) != 0;
        ship.IsThrusting = (flags & IsThrustingFlag) != 0;
    }

    private static void ReadPhysics(SnapshotReader reader, Ship ship)
    {
        ship.Score = reader.ReadFloat();
        ship.X = reader.ReadFloat();
        ship.Y = reader.ReadFloat();
        ship.VelocityX = reader.ReadFloat();
        ship.VelocityY = reader.ReadFloat();
        ship.Angle = reader.ReadFloat();
        ship.WeaponAngle = reader.ReadFloat();
        ship.Energy = reader.ReadFloat();
        ship.Fuel = reader.ReadFloat();
        ship.TeleportX = reader.ReadFloat();
        ship.TeleportY = reader.ReadFloat();
        ship.PropellerAngle = reader.ReadFloat();
        ship.WeaponCharge = reader.ReadFloat();
        ship.WeaponCounter = reader.ReadFloat();
    }

    private static void ReadFloats(SnapshotReader reader, float[] values)
    {
        for (var i = 0; i < values.Length; i++)
        {
            values[i] = reader.ReadFloat();
        }
    }
}
