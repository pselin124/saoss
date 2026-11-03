namespace Saoss.Core.Simulation.Weapons;

/// <summary>
/// Weapon behavior of one ship type. The base class does nothing, which is the behavior of a ship
/// without that weapon feature. Each hook matches one function of the original <c>Player</c> class.
/// </summary>
public class ShipBehavior
{
    /// <summary>Fires the primary weapon; cooldown and fuel were already checked [ammu].</summary>
    public virtual void Fire(Ship ship, World world)
    {
    }

    /// <summary>Activates the special; cooldown and fuel were already checked [ease].</summary>
    public virtual void ActivateSpecial(Ship ship, World world)
    {
    }

    /// <summary>Per-tick special effects that run during movement, before the position update.</summary>
    public virtual void BeforePositionUpdate(Ship ship, World world)
    {
    }

    /// <summary>Moves primary projectiles and resolves their hits [liikutaPadit].</summary>
    public virtual void UpdatePrimaryProjectiles(Ship ship, World world)
    {
    }

    /// <summary>Moves special projectiles and resolves their hits [liikutaePadit].</summary>
    public virtual void UpdateSpecialProjectiles(Ship ship, World world)
    {
    }

    /// <summary>Lets this ship's projectiles damage other ships' projectiles [tarkistaHajosikoPadit].</summary>
    public virtual void UpdateProjectileClashes(Ship ship, World world)
    {
    }
}
