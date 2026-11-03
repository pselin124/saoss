using Saoss.Core;
using Saoss.Core.Simulation;

namespace Saoss.Game.Rendering.Scene;

/// <summary>
/// Background stars [star array]: the first 75 spell "THP" just below the arena and ripple in a wave;
/// the rest are scattered far below.
/// </summary>
public sealed class Starfield
{
    private const int StarCount = 500;
    private const int LetterStarCount = 75;

    /// <summary>Creates the stars with a fixed layout (letters) and random positions for the rest.</summary>
    public Starfield(GameRandom random)
    {
        Stars = new Star[StarCount];
        var index = 0;
        void Letter(int count, Func<int, (float X, float Y)> position)
        {
            for (var i = 0; i < count; i++)
            {
                var (x, y) = position(i);
                var green = random.Next(0.1f) + 0.6f;
                Stars[index++] = new Star { X = x, Y = y, Z = -10f, Red = random.Next(0.1f) + 0.6f, Green = green, Blue = green, Size = 0.3f };
            }
        }

        // T, H and P made of short star lines, as laid out in alota().
        Letter(9, i => (i - 60f, 50f));
        Letter(10, i => (-56f, 40f + i));
        Letter(11, i => (-50f, 40f + i));
        Letter(11, i => (-41f, 40f + i));
        Letter(8, i => (-49f + i, 45f));
        Letter(11, i => (-38f, 40f + i));
        Letter(5, i => (-37f + i, 50f));
        Letter(5, i => (-37f + i, 45f));
        Letter(4, i => (-33f, 46f + i));
        // One letter slot was never set in the original and stayed an invisible zero-size star.
        while (index < LetterStarCount)
        {
            Stars[index++] = new Star { Size = 0f };
        }

        for (; index < StarCount; index++)
        {
            var green = random.Next(0.25f) + 0.75f;
            Stars[index] = new Star
            {
                X = random.Next(GameConstants.ArenaMaxX * 8) - (GameConstants.ArenaMaxX * 4),
                Y = random.Next(GameConstants.ArenaMaxY * 8) - (GameConstants.ArenaMaxY * 4),
                Z = (-random.Next(50f)) - 180f,
                Red = random.Next(0.1f) + 0.9f,
                Green = green,
                Blue = green,
                Size = 1f,
            };
        }
    }

    /// <summary>All stars.</summary>
    public Star[] Stars { get; }

    /// <summary>Moves the letter stars in their ripple for the given tick.</summary>
    public void Update(int tick)
    {
        for (var i = 0; i < LetterStarCount; i++)
        {
            var dx = Stars[i].X + 45f;
            var dy = Stars[i].Y - 45f;
            Stars[i].Z = (MathF.Sin((((dx * dx) + (dy * dy)) / 70f) + (tick / 50f)) * -8f) - 50f;
        }
    }

    /// <summary>One background star; Z is relative to the ship plane.</summary>
    public struct Star
    {
        /// <summary>Position.</summary>
        public float X;

        /// <inheritdoc cref="X"/>
        public float Y;

        /// <inheritdoc cref="X"/>
        public float Z;

        /// <summary>Color.</summary>
        public float Red;

        /// <inheritdoc cref="Red"/>
        public float Green;

        /// <inheritdoc cref="Red"/>
        public float Blue;

        /// <summary>Half size of the quad.</summary>
        public float Size;
    }
}
