using Saoss.Core.Ships;

namespace Saoss.Core.Simulation.Ai;

/// <summary>
/// The computer pilot [ailiiku]. Once per tick, before anything moves, it picks the nearest enemy and sets
/// the ship's <see cref="Ship.Controls"/> to chase, aim at and shoot it while keeping clear of the planet.
/// The original pilot has no memory of its own; a few decisions read the controls it chose on the previous tick.
/// Easier <see cref="AiDifficulty"/> levels add the handicaps of <see cref="AiSkill"/>, kept in <see cref="Ship.Ai"/>.
/// </summary>
public static class AiPilot
{
    /// <summary>Sets the controls of one computer-controlled ship for this tick.</summary>
    public static void Update(Ship ship, World world)
    {
        var decision = new Decision(ship, world);
        decision.Run();
        ship.Controls = decision.Controls;
    }

    private sealed class Decision
    {
        // The original's PI is a float constant.
        private const double Pi = MathF.PI;
        private const double NoTargetDistanceSquared = 900_000_000;
        private const int TargetableDeathTimer = 20;

        private const double MinLeadSpeed = 0.05;
        private const double MaxLeadSpeed = 4;
        private const double MaxAcceleratingShotLeadRadius = 2;

        private const double ChaseAimTolerance = 45;
        private const double ChaseDistanceFactor = 2.0 / 3.0;
        private const double LeechmotherStandOffFactor = 1.2;
        private const double DefaultThrustAimTolerance = 90;
        private const double BulldogRamDistance = 15;
        private const double BulldogRamAimTolerance = 10;
        private const double PickupSteerSpeedDivisor = 10;

        private const double StarWormCloseDistance = 50;
        private const double StarWormMediumDistance = 75;
        private const double RammerTurnAwayAimTolerance = 90;
        private const double RammerTurnAwayDistance = 40;
        private const double AsteroidSearchDistance = 10_000;
        private const double AsteroidEdgeMargin = 10;

        private const double BasePlanetClearance = 35;
        private const double MassPlanetClearanceBonus = 10;
        private const double MassPlanetClearanceDivisor = 50;
        private const double SmallPlanetClearance = 35;
        private const double WidePlanetClearance = 45;
        private const double WiiiiPlanetClearance = 40;
        private const double LargePlanetClearance = 50;
        private const double PlanetGravityExponent = 2.3;
        private const double PlanetAheadAngle = Pi / 3;
        private const double PlanetAheadSteerAngle = Pi / 4;
        private const double PlanetAheadHeadingAngle = Pi / 5;
        private const double DriftAlignedAngle = Pi / 20;

        private const double FireAimTolerance = 30;
        private const double CloseWeaponAimTolerance = 35;
        private const double LeechmotherAimTolerance = 7;
        private const double LeechmotherShotSteerAngle = Pi / 3;
        private const double PowerVacuumAimTolerance = 3;
        private const double PowerVacuumRangeFactor = 0.8;
        private const double PowerVacuumReleaseCharge = 250;
        private const double BulldogAimTolerance = 10;
        private const double WiiiiFullFuelMargin = 1;
        private const double WiiiiCloseDistance = 40;
        private const double CubixFullFuelMargin = 2;
        private const double SideAimAngle = Pi / 4;
        private const double SpaceTurtleWeakTargetEnergy = 30;
        private const double LargeShotRadius = 4;
        private const double SlowShotSpeed = 0.03;
        private const double ElectraKeepFuelRadiusMargin = 1.5;

        private const double MagnetoProjectileMargin = 1;
        private const double MagnetoSpecialProjectileMargin = 2;
        private const double WaspStingDistance = 3;
        private const double WaspBehindDistance = 7;
        private const double WaspBehindAngle = 135.0 / 180.0 * Pi;
        private const double RearAngleDegrees = 90;
        private const double UfoReachMargin = 2.5;
        private const double ShapperShieldMargin = 5;
        private const double ShapperOpenMargin = 15;
        private const double WiiiiBoostDistance = 30;
        private const double WiiiiBoostAimAngle = Pi / 6;
        private const double ElephantReachFactor = 1.5;
        private const double TailAngle = Pi - (Pi / 6);
        private const double CubixCloseDistance = 15;
        private const double CubixContactDistance = 10;
        private const double TonguReachFactor = 0.785;
        private const double SpaceTurtleReachDivisor = 1.75;
        private const double StarWormSpecialAngle = Pi / 2;

        private readonly Ship _ship;
        private readonly World _world;
        private readonly ShipDefinition _definition;
        private readonly AiSkill _skill;
        private ShipControls _controls;
        private Ship? _target;
        private double _dx;
        private double _dy;
        private double _targetAngle;
        private double _distance;
        private double _leadAngle;
        private double _heading;
        private bool _steeringChosen;
        private bool _planetSpecialChosen;

        public Decision(Ship ship, World world)
        {
            _ship = ship;
            _world = world;
            _definition = ship.Definition;
            _skill = AiSkill.For(world.Settings.AiDifficulty);
            _controls = ship.Controls;
            _heading = ship.Angle / 180f * MathF.PI;
        }

        public ShipControls Controls => _controls;

        private ShipType Type => _ship.Type;

        public void Run()
        {
            SelectTarget();
            if (!_skill.IsFlawless)
            {
                AiHandicap.Update(_ship.Ai, _skill, _target, _world.Random);
            }

            if (_target is not null)
            {
                MeasureTarget(_target);
                ComputeLeadAngle(_target);
                if (!_skill.IsFlawless)
                {
                    Misjudge();
                }

                DecideThrust(_target);
                if (!_skill.IsFlawless && _ship.Ai.IsHoldingBack)
                {
                    _controls.Thrust = false;
                }

                if (!_steeringChosen)
                {
                    DecideTurning();
                }
            }

            AvoidPlanet();

            if (_target is not null)
            {
                DecideFire(_target);
                DecideSpecial(_target);
                if (!_skill.IsFlawless)
                {
                    Hesitate();
                }
            }
            else
            {
                // Quirk: without a target, thrust and turning keep their values from the previous tick.
                _controls.Fire = false;
                _controls.Special = false;
            }
        }

        // The nearest living enemy; failing that (unless disabled), the nearest other ship of any team.
        private void SelectTarget()
        {
            var best = NoTargetDistanceSquared;
            foreach (var other in _world.Ships)
            {
                if (other != _ship && other.IsInGame && other.DeathTimer < TargetableDeathTimer && other.Team != _ship.Team)
                {
                    ConsiderTarget(other, ref best);
                }
            }

            if (_target is null && !_world.Settings.AiTargetsOnlyEnemies)
            {
                foreach (var other in _world.Ships)
                {
                    // Quirk: this fallback does not skip destroyed ships.
                    if (other != _ship && other.IsInGame)
                    {
                        ConsiderTarget(other, ref best);
                    }
                }
            }
        }

        private void ConsiderTarget(Ship other, ref double best)
        {
            var dx = other.X - _ship.X;
            var dy = other.Y - _ship.Y;
            var distanceSquared = (dx * dx) + (dy * dy);
            if (distanceSquared < best)
            {
                best = distanceSquared;
                _target = other;
            }
        }

        private void MeasureTarget(Ship target)
        {
            _dx = target.X - _ship.X;
            _dy = target.Y - _ship.Y;
            _targetAngle = Math.Atan2(_dy, _dx);
            _distance = Math.Sqrt((_dx * _dx) + (_dy * _dy));
        }

        /// <summary>
        /// Direction to aim so the primary shot meets the moving target. Steps: estimate where the target will be
        /// when a shot arrives (constant-speed shots, or accelerating shots for very slow speeds), then aim there
        /// if the shot can reach, else aim straight at the target.
        /// </summary>
        private void ComputeLeadAngle(Ship target)
        {
            var speed = Exact(_definition.Primary.Speed);
            var (targetVelocityX, targetVelocityY) = _skill.IsFlawless
                ? ((double)target.VelocityX, (double)target.VelocityY)
                : (_ship.Ai.TargetVelocityX * _skill.LeadFactor, _ship.Ai.TargetVelocityY * _skill.LeadFactor);
            var isConstantSpeedShot = speed > MinLeadSpeed && speed < MaxLeadSpeed;
            var isAcceleratingShot = speed < MinLeadSpeed;
            var leadDistance = 0.0;

            // Quirk: the shot speed is offset by the shooter's velocity per axis instead of as a vector.
            var shotSpeedX = speed + _ship.VelocityX;
            var shotSpeedY = speed + _ship.VelocityY;
            if (isConstantSpeedShot)
            {
                var leadX = (targetVelocityX * _distance / shotSpeedX) + _dx;
                var leadY = (targetVelocityY * _distance / shotSpeedY) + _dy;
                leadDistance = Math.Sqrt((leadY * leadY) + (leadX * leadX));
            }
            else if (isAcceleratingShot)
            {
                // Quirk: this is only the target's displacement, not the aim point.
                var travelTime = Math.Sqrt(_distance / (0.5 * speed));
                var leadX = targetVelocityX * travelTime;
                var leadY = targetVelocityY * travelTime;
                leadDistance = Math.Sqrt((leadY * leadY) + (leadX * leadX));
            }

            if (isConstantSpeedShot)
            {
                _leadAngle = leadDistance > speed * _definition.Primary.Lifetime
                    ? _targetAngle
                    : Math.Atan2(_dy + (targetVelocityY * leadDistance / shotSpeedY), _dx + (targetVelocityX * leadDistance / shotSpeedX));
            }
            else if (isAcceleratingShot && Exact(_definition.Primary.Radius) <= MaxAcceleratingShotLeadRadius)
            {
                var travelTime = Math.Sqrt(leadDistance / (0.5 * speed));
                _leadAngle = Math.Atan2(_dy + (targetVelocityY * travelTime), _dx + (targetVelocityX * travelTime));
            }
            else
            {
                _leadAngle = _targetAngle;
            }
        }

        // Handicap: aim off by the current error and judge every distance threshold scaled by the current factor.
        private void Misjudge()
        {
            var state = _ship.Ai;
            var aimError = state.AimErrorDegrees / 180.0 * Pi;
            _leadAngle += aimError;
            _targetAngle += aimError;
            _distance /= state.DistanceScale;
        }

        // Handicap: a passive mood drops planned shots and specials (not a planet boost); now and then it fires anyway.
        private void Hesitate()
        {
            var holdingBack = _ship.Ai.IsHoldingBack;
            _controls.Fire = (_controls.Fire && !holdingBack) || AiHandicap.FiresWildly(_skill, _world.Random);
            _controls.Special = _planetSpecialChosen || (_controls.Special && !holdingBack);
        }

        private void DecideThrust(Ship target)
        {
            var aimError = AimErrorDegrees(_targetAngle);
            var range = Range(_definition);
            if (Type is ShipType.Magneto or ShipType.Elephant)
            {
                _controls.Thrust = aimError < ChaseAimTolerance && _distance > range * ChaseDistanceFactor;
            }
            else if (Type == ShipType.Leechmother)
            {
                _controls.Thrust = aimError < ChaseAimTolerance && _distance > Range(target.Definition) * LeechmotherStandOffFactor;
            }
            else if (Type is ShipType.ScatterRay or ShipType.StickInsect)
            {
                _controls.Thrust = true;
            }
            else if (Type == ShipType.Vampire)
            {
                ChasePickups();
                if (!_steeringChosen)
                {
                    _controls.Thrust = aimError < ChaseAimTolerance && _distance > range * ChaseDistanceFactor;
                }
            }
            else if (Type == ShipType.Bulldog)
            {
                _controls.Thrust = (_distance < Range(target.Definition) || _distance < BulldogRamDistance) && aimError < BulldogRamAimTolerance;
            }
            else
            {
                _controls.Thrust = aimError < DefaultThrustAimTolerance;
            }

            if (Type == ShipType.Electra)
            {
                _controls.Thrust = false;
            }
        }

        // Vampire flies to pickups; when moving fast it steers its velocity rather than its nose toward them.
        private void ChasePickups()
        {
            foreach (var pickup in _world.Pickups)
            {
                if (!pickup.IsActive)
                {
                    continue;
                }

                // Quirk: every active pickup overwrites the steering, so the last one in the pool wins.
                _controls.Thrust = true;
                _steeringChosen = true;
                var dx = (double)(pickup.X - _ship.X);
                var dy = (double)(pickup.Y - _ship.Y);
                var pickupAngle = ToDegrees(Math.Atan2(dy, dx));
                var speed = Math.Sqrt(((double)_ship.VelocityX * _ship.VelocityX) + ((double)_ship.VelocityY * _ship.VelocityY));
                var reference = speed > Math.Sqrt((dx * dx) + (dy * dy)) / PickupSteerSpeedDivisor
                    ? ToDegrees(Math.Atan2(_ship.VelocityY, _ship.VelocityX))
                    : _ship.Angle;
                SetTurn(left: IsCounterClockwiseOf(pickupAngle, reference, 180));
            }
        }

        /// <summary>
        /// Turns toward the target. Steps by ship type: Star Worm keeps its distance, Electra turns to the target
        /// only with enough fuel to shoot (else to the nearest asteroid), close-range ships aim straight at the
        /// target, rammers turn away when close behind, and everyone else aims at the lead point.
        /// </summary>
        private void DecideTurning()
        {
            if (Type == ShipType.StarWorm)
            {
                var keepAway = _distance < StarWormCloseDistance
                    || (Math.Abs(_heading - _leadAngle) > Pi / 2 && _distance < StarWormMediumDistance);
                var isCounterClockwise = IsCounterClockwiseOf(_heading, _leadAngle, Pi);
                SetTurn(left: keepAway ? isCounterClockwise : !isCounterClockwise);
            }
            else if (Type == ShipType.Electra)
            {
                if (_ship.Fuel >= Exact(_definition.Primary.FuelCost))
                {
                    SetTurn(left: IsCounterClockwiseOf(_targetAngle, _heading, Pi));
                }
                else
                {
                    TurnTowardNearestAsteroid();
                }
            }
            else if ((Type == ShipType.Moth && _distance > Range(_definition))
                || Type is ShipType.Ufo or ShipType.PeaceCannon or ShipType.Wiiii or ShipType.Bulldog)
            {
                SetTurn(left: IsCounterClockwiseOf(ToDegrees(_targetAngle), _ship.Angle, 180));
            }
            else if ((Type != ShipType.ScatterRay && Type != ShipType.StickInsect)
                || AimErrorDegrees(_targetAngle) < RammerTurnAwayAimTolerance
                || _distance > RammerTurnAwayDistance)
            {
                SetTurn(left: IsCounterClockwiseOf(ToDegrees(_leadAngle), _ship.Angle, 180));
            }
            else
            {
                SetTurn(left: !IsCounterClockwiseOf(ToDegrees(_leadAngle), _ship.Angle, 180));
            }
        }

        // Electra low on fuel turns to the nearest asteroid well inside the arena.
        private void TurnTowardNearestAsteroid()
        {
            var best = AsteroidSearchDistance;
            var nearestX = 0.0;
            var nearestY = 0.0;
            foreach (var asteroid in _world.Asteroids.Asteroids)
            {
                var dx = (double)(asteroid.X - _ship.X);
                var dy = (double)(asteroid.Y - _ship.Y);
                var distance = Math.Sqrt((dx * dx) + (dy * dy));
                if (distance < best
                    && asteroid.X < GameConstants.ArenaMaxX - AsteroidEdgeMargin
                    && asteroid.X > GameConstants.ArenaMinX + AsteroidEdgeMargin
                    && asteroid.Y < GameConstants.ArenaMaxY - AsteroidEdgeMargin
                    && asteroid.Y > GameConstants.ArenaMinY + AsteroidEdgeMargin)
                {
                    nearestX = dx;
                    nearestY = dy;
                    best = distance;
                }
            }

            SetTurn(left: IsCounterClockwiseOf(Math.Atan2(nearestY, nearestX), _heading, Pi));
        }

        /// <summary>
        /// Keeps the ship off the planet; runs even without a target. Steps: unwrap the velocity angle and the
        /// heading to within half a turn of the planet direction, thrust when inside the clearance, then handle
        /// the planet behind the ship and the planet ahead of its velocity.
        /// </summary>
        private void AvoidPlanet()
        {
            var clearance = PlanetClearance();
            double x = _ship.X;
            double y = _ship.Y;
            var velocityAngle = Math.Atan2(_ship.VelocityY, _ship.VelocityX);
            var planetAngle = Math.Atan2(-1 * y, -1 * x);
            velocityAngle = Unwrap(velocityAngle, planetAngle);

            // The shifted heading is also used by the fire and special decisions.
            _heading = Unwrap(_heading, planetAngle);

            var distance = Math.Sqrt((x * x) + (y * y));
            if (distance < clearance)
            {
                _controls.Thrust = true;
            }

            if (distance < clearance / 2 && Math.Abs(_heading - planetAngle) > Pi / 2)
            {
                AvoidPlanetBehind(x, y, distance, velocityAngle, planetAngle);
            }

            if (distance < clearance && Math.Abs(velocityAngle - planetAngle) < PlanetAheadAngle)
            {
                AvoidPlanetAhead(velocityAngle, planetAngle);
            }
        }

        private double PlanetClearance()
        {
            var clearance = BasePlanetClearance + MassPlanetClearanceBonus - (Exact(_definition.Mass) / MassPlanetClearanceDivisor);
            return Type switch
            {
                ShipType.Ufo or ShipType.Electra or ShipType.StarWorm => SmallPlanetClearance,
                ShipType.Tringu or ShipType.Magneto => WidePlanetClearance,
                ShipType.Wiiii => WiiiiPlanetClearance,
                ShipType.Cubix or ShipType.Wasp => LargePlanetClearance,
                _ => clearance,
            };
        }

        // The planet is behind: if the ship drifts toward it, turn (or boost) so the thrust carries it away.
        private void AvoidPlanetBehind(double x, double y, double distance, double velocityAngle, double planetAngle)
        {
            var pull = Math.Pow(distance, PlanetGravityExponent);
            var gravity = Exact(GameConstants.Gravity);
            var massRatio = Exact(GameConstants.PlanetMass) / Exact(_definition.Mass);

            // Quirk: gravity is added pointing away from the planet, so this predicts the drift wrongly.
            var driftX = _ship.VelocityX + (x / pull * gravity * massRatio);
            var driftY = _ship.VelocityY + (y / pull * gravity * massRatio);
            var driftError = Math.Abs(Math.Atan2(driftY, driftX) - planetAngle);
            var driftsTowardPlanet = driftError < Pi / 2;
            if (Math.Abs(_heading - velocityAngle) < DriftAlignedAngle && driftError > Pi / 2)
            {
                _controls.Left = false;
                _controls.Right = false;
            }
            else if (driftsTowardPlanet && Type is ShipType.Bulldog or ShipType.Wiiii or ShipType.Cubix)
            {
                _controls.Special = true;
                _planetSpecialChosen = true;
            }
            else if (_heading - velocityAngle < 0 && driftsTowardPlanet)
            {
                SetTurn(left: true);
            }
            else if (driftsTowardPlanet)
            {
                SetTurn(left: false);
            }
        }

        // The ship is moving toward the planet: turn the nose across the velocity, or stop turning.
        private void AvoidPlanetAhead(double velocityAngle, double planetAngle)
        {
            var steer = Math.Abs(velocityAngle - planetAngle) < PlanetAheadSteerAngle
                && Math.Abs(velocityAngle - _heading) < PlanetAheadHeadingAngle;
            if (steer)
            {
                SetTurn(left: velocityAngle - planetAngle > 0);
            }
            else
            {
                _controls.Left = false;
                _controls.Right = false;
            }
        }

        /// <summary>
        /// Primary weapon trigger. Steps by weapon, first match wins: shielded Shapper, plain guns (in range and
        /// aimed), Ufo/Electra (enemy shots or the enemy within reach), Peace Cannon, unshielded Shapper, Leechmother
        /// (aim, then hold while the shot heads to the target), Power Vacuum (charge and release), Bulldog, Wiiii,
        /// Cubix, Tongu, Space Turtle, then generic rules by shot size and speed.
        /// </summary>
        private void DecideFire(Ship target)
        {
            var range = Range(_definition);
            var leadError = AimErrorDegrees(_leadAngle);
            var aimError = AimErrorDegrees(_targetAngle);
            var maxFuel = Exact(_definition.MaxFuel);
            var primaryRadius = Exact(_definition.Primary.Radius);
            bool fire;
            if (Type == ShipType.Shapper && _ship.IsShieldActive)
            {
                fire = _distance < range && leadError < FireAimTolerance
                    && _ship.Fuel >= Exact(_definition.Special.FuelCost) + Exact(_definition.Primary.FuelCost);
            }
            else if (Type is ShipType.Magneto or ShipType.Tringu or ShipType.Wasp or ShipType.Twinblade or ShipType.Vampire
                or ShipType.ScatterRay or ShipType.Elephant or ShipType.StickInsect)
            {
                fire = _distance < range && leadError < FireAimTolerance;
            }
            else if (Type is ShipType.Ufo or ShipType.Electra)
            {
                fire = ShouldFireZapper(target, primaryRadius);
            }
            else if (Type == ShipType.PeaceCannon)
            {
                fire = _distance < primaryRadius + Exact(target.Definition.Radius) && aimError <= CloseWeaponAimTolerance;
            }
            else if (Type == ShipType.Shapper)
            {
                fire = _distance < Exact(_definition.Special.Radius) && aimError <= CloseWeaponAimTolerance;
            }
            else if (Type == ShipType.Leechmother)
            {
                fire = ShouldFireLeechmother(target);
            }
            else if (Type == ShipType.PowerVacuum)
            {
                var isChargedAndAimed = leadError < PowerVacuumAimTolerance
                    && _distance < range * PowerVacuumRangeFactor
                    && _ship.PrimaryProjectiles[0].Energy > PowerVacuumReleaseCharge;
                fire = !_controls.Fire || !isChargedAndAimed;
            }
            else if (Type == ShipType.Bulldog)
            {
                fire = aimError < BulldogAimTolerance;
            }
            else if (Type == ShipType.Wiiii)
            {
                fire = _ship.Fuel >= maxFuel - WiiiiFullFuelMargin || _distance < WiiiiCloseDistance;
            }
            else if (Type == ShipType.Cubix)
            {
                fire = _distance < range && Math.Abs(_heading - _targetAngle) < SideAimAngle && _ship.Fuel >= maxFuel - CubixFullFuelMargin;
            }
            else if (Type == ShipType.Tongu)
            {
                fire = _distance < range && Math.Abs(_heading - _targetAngle) < SideAimAngle && _ship.Fuel >= maxFuel / 2;
            }
            else if (Type == ShipType.SpaceTurtle)
            {
                // Quirk: checks the target type's maximum energy, not its current energy.
                fire = _distance < range && Math.Abs(_leadAngle - _targetAngle) < SideAimAngle
                    && (_ship.Fuel >= maxFuel / 2.0f || Exact(target.Definition.MaxEnergy) >= SpaceTurtleWeakTargetEnergy);
            }
            else if (primaryRadius > LargeShotRadius)
            {
                fire = _distance < primaryRadius && leadError < FireAimTolerance;
            }
            else if (Exact(_definition.Primary.Speed) < SlowShotSpeed)
            {
                fire = _distance < 0.5 * _definition.Primary.Lifetime * _definition.Primary.Lifetime * Exact(_definition.Primary.Speed) && _controls.Thrust;
            }
            else
            {
                fire = _distance < range && _controls.Thrust;
            }

            _controls.Fire = fire;
        }

        // Ufo and Electra zap enemy shots that come near and the enemy when it is within reach.
        private bool ShouldFireZapper(Ship target, double primaryRadius)
        {
            var targetDefinition = target.Definition;
            var fire = false;
            if (targetDefinition.Primary.CanBeDestroyed
                && AnyActiveWithin(target.PrimaryProjectiles, targetDefinition.Primary.MaxProjectiles, primaryRadius))
            {
                fire = true;
            }

            if (targetDefinition.Special.CanBeDestroyed
                && AnyActiveWithin(target.SpecialProjectiles, targetDefinition.Special.MaxProjectiles, primaryRadius))
            {
                fire = true;
            }

            if (_distance < primaryRadius + Exact(targetDefinition.Radius))
            {
                fire = true;
            }

            if (_distance < Exact(targetDefinition.Radius) + ElectraKeepFuelRadiusMargin
                && _ship.Fuel < Exact(_definition.Special.FuelCost) + Exact(_definition.Primary.FuelCost))
            {
                fire = false;
            }

            return fire;
        }

        // Leechmother fires when aimed, then keeps the trigger held while its shot is still heading toward the target.
        private bool ShouldFireLeechmother(Ship target)
        {
            var shot = _ship.PrimaryProjectiles[0];
            if (!shot.IsActive)
            {
                return AimErrorDegrees(_leadAngle) < LeechmotherAimTolerance;
            }

            var shotAngle = Math.Atan2(shot.VelocityY, shot.VelocityX);
            var shotToTarget = Math.Atan2(target.Y - shot.Y, target.X - shot.X);
            return Math.Abs(shotAngle - shotToTarget) < LeechmotherShotSteerAngle;
        }

        /// <summary>
        /// Special weapon trigger, one rule per ship type. Several rules only fire when neither weapon was held,
        /// which makes them pulse on alternate ticks. A boost chosen by the planet check always wins.
        /// </summary>
        private void DecideSpecial(Ship target)
        {
            var targetDefinition = target.Definition;
            var special = _definition.Special;
            var radius = Exact(_definition.Radius);
            var targetRadius = Exact(targetDefinition.Radius);
            var maxFuel = Exact(_definition.MaxFuel);
            var aimError = AimErrorDegrees(_targetAngle);
            var headingError = Math.Abs(_heading - _targetAngle);
            var specialReach = special.Duration * Exact(special.Speed);
            var held = _controls.Fire || _controls.Special;
            switch (Type)
            {
                case ShipType.Magneto:
                    _controls.Special = ShouldRaiseShield(target);
                    break;
                case ShipType.Tringu:
                    _controls.Special = _distance < specialReach && !held;
                    break;
                case ShipType.Wasp:
                    _controls.Special = ShouldSting(target);
                    break;
                case ShipType.Twinblade:
                    _controls.Special = aimError > RearAngleDegrees && _distance < Range(_definition) && !held;
                    break;
                case ShipType.Leechmother:
                    if (!_controls.Fire)
                    {
                        _controls.Special = true;
                    }

                    break;
                case ShipType.Trek:
                    _controls.Special = _ship.Energy > Exact(_definition.MaxEnergy) / 2 && _distance > Range(_definition)
                        && target.Type != ShipType.Thunderbolt;
                    break;
                case ShipType.Ufo:
                    _controls.Special = _distance < ((radius + targetRadius) / 4 * Pi) + UfoReachMargin;
                    break;
                case ShipType.Thunderbolt:
                case ShipType.Electra:
                    _controls.Special = true;
                    break;
                case ShipType.ScatterRay:
                case ShipType.StickInsect:
                    _controls.Special = aimError > RearAngleDegrees && !held;
                    break;
                case ShipType.PeaceCannon:
                    _controls.Special = _distance > Exact(_definition.Primary.Radius) && !_controls.Thrust;
                    break;
                case ShipType.Vampire:
                    _controls.Special = _distance < Exact(special.Radius);
                    break;
                case ShipType.Shapper:
                    _controls.Special = _ship.IsShieldActive
                        ? _distance < Exact(special.Radius) + ShapperShieldMargin
                        : _distance > Exact(special.Radius) + ShapperOpenMargin;
                    break;
                case ShipType.Moth:
                    _controls.Special = _ship.Fuel < maxFuel;
                    break;
                case ShipType.PowerVacuum:
                    _controls.Special = target.SlimeCount < GameConstants.MaxSlimes && _distance < Exact(special.Speed) * special.Duration;
                    break;
                case ShipType.Bulldog:
                    _controls.Special = (_distance < BulldogRamDistance && aimError < BulldogRamAimTolerance) || _planetSpecialChosen;
                    break;
                case ShipType.Wiiii:
                    _controls.Special = (!_controls.Special && _distance < WiiiiBoostDistance && _ship.Fuel < maxFuel / 3
                        && headingError < WiiiiBoostAimAngle) || _planetSpecialChosen;
                    break;
                case ShipType.Elephant:
                    _controls.Special = _distance < specialReach * ElephantReachFactor && headingError > TailAngle;
                    break;
                case ShipType.Cubix:
                    _controls.Special = (_distance < CubixCloseDistance && headingError > TailAngle)
                        || _distance < CubixContactDistance || _planetSpecialChosen || _ship.Fuel >= maxFuel;
                    break;
                case ShipType.Tongu:
                    _controls.Special = _distance < (radius + targetRadius) * TonguReachFactor && headingError < SideAimAngle;
                    break;
                case ShipType.SpaceTurtle:
                    // Quirk: checks the target type's maximum energy, not its current energy.
                    _controls.Special = _distance < Exact(special.Speed) * special.Duration / SpaceTurtleReachDivisor
                        && Exact(targetDefinition.MaxEnergy) < SpaceTurtleWeakTargetEnergy;
                    break;
                case ShipType.StarWorm:
                    _controls.Special = Math.Abs(_leadAngle - _targetAngle) < StarWormSpecialAngle
                        && (_distance < StarWormMediumDistance || _ship.Fuel >= maxFuel / 2);
                    break;
                case ShipType.Popper:
                    _controls.Special = false;
                    break;
            }
        }

        // Magneto shields itself when the target or any of the target's shots is about to hit.
        private bool ShouldRaiseShield(Ship target)
        {
            var targetDefinition = target.Definition;
            var radius = Exact(_definition.Radius);
            var targetShotRadius = Exact(targetDefinition.Primary.Radius);
            if ((_dx * _dx) + (_dy * _dy) < Math.Pow(targetShotRadius, 2)
                || (target.Type == ShipType.Bulldog && _distance < Exact(targetDefinition.Radius) + radius + MagnetoProjectileMargin))
            {
                return true;
            }

            return AnyActiveWithin(target.PrimaryProjectiles, targetDefinition.Primary.MaxProjectiles, radius + targetShotRadius + MagnetoProjectileMargin)
                || AnyActiveWithin(target.SpecialProjectiles, targetDefinition.Special.MaxProjectiles,
                    radius + Exact(targetDefinition.Special.Radius) + MagnetoSpecialProjectileMargin);
        }

        // Wasp stings when touching the target or when close behind it, unless the target is a shielded Magneto.
        private bool ShouldSting(Ship target)
        {
            if (target.Type == ShipType.Magneto && target.IsShieldActive)
            {
                return false;
            }

            if (_distance < WaspStingDistance && target.DeathTimer <= 0)
            {
                return true;
            }

            // Quirk: compares the target's heading in degrees with an angle in radians.
            return Math.Abs(target.Angle - _targetAngle) > WaspBehindAngle && _distance < WaspBehindDistance && target.DeathTimer <= 0;
        }

        private bool AnyActiveWithin(Projectile[] pool, int count, double radius)
        {
            for (var i = 0; i < count; i++)
            {
                var projectile = pool[i];
                if (projectile.IsActive)
                {
                    double dx = projectile.X - _ship.X;
                    double dy = projectile.Y - _ship.Y;
                    if (Math.Sqrt((dx * dx) + (dy * dy)) < radius)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        // Quirk: angle differences are never wrapped, so targets across the +-180 degree seam look far off.
        private double AimErrorDegrees(double radians) => Math.Abs(ToDegrees(radians) - _ship.Angle);

        private void SetTurn(bool left)
        {
            _controls.Left = left;
            _controls.Right = !left;
        }

        private static bool IsCounterClockwiseOf(double angle, double reference, double halfTurn) =>
            (angle > reference && angle < reference + halfTurn) || angle < reference - halfTurn;

        private static double Unwrap(double angle, double reference)
        {
            if (reference - angle > Pi)
            {
                angle += 2 * Pi;
            }

            if (reference - angle < -Pi)
            {
                angle -= 2 * Pi;
            }

            return angle;
        }

        private static double Range(ShipDefinition definition) => definition.Primary.Lifetime * Exact(definition.Primary.Speed);

        private static double ToDegrees(double radians) => radians * 180.0 / Pi;

        // The original stats were doubles; going through decimal recovers e.g. exactly 0.05 from 0.05f.
        private static double Exact(float value) => (double)(decimal)value;
    }
}
