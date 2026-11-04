namespace Saoss.Core.Simulation;

/// <summary>
/// Deterministic random source for the simulation, so a match can be replayed from its seed. It produces exactly
/// the sequence of <c>new System.Random(seed)</c> (Knuth's subtractive generator) but keeps the state in the open,
/// so a world snapshot can carry it.
/// </summary>
public sealed class GameRandom
{
    /// <summary>Length of the generator state; index 0 is unused, as in System.Random.</summary>
    internal const int StateLength = 56;

    private const int SeedBase = 161803398;
    private const int FirstNextP = 21;
    private const int SeedStride = 21;
    private const int ShuffleOffset = 30;
    private const int ShuffleRounds = 4;
    private const int LastIndex = 55;

    private readonly int[] _state = new int[StateLength];

    /// <summary>Creates the generator; the same seed gives the same sequence as <c>new System.Random(seed)</c>.</summary>
    public GameRandom(int seed)
    {
        var subtraction = seed == int.MinValue ? int.MaxValue : Math.Abs(seed);
        var mj = SeedBase - subtraction;
        _state[LastIndex] = mj;
        var mk = 1;
        var ii = 0;
        for (var i = 1; i < LastIndex; i++)
        {
            ii = (ii + SeedStride) % LastIndex;
            _state[ii] = mk;
            mk = mj - mk;
            if (mk < 0)
            {
                mk += int.MaxValue;
            }

            mj = _state[ii];
        }

        for (var k = 0; k < ShuffleRounds; k++)
        {
            for (var i = 1; i < StateLength; i++)
            {
                _state[i] -= _state[1 + ((i + ShuffleOffset) % LastIndex)];
                if (_state[i] < 0)
                {
                    _state[i] += int.MaxValue;
                }
            }
        }

        NextPIndex = FirstNextP;
    }

    /// <summary>Generator state [1..55]; index 0 is unused.</summary>
    internal ReadOnlySpan<int> State => _state;

    /// <summary>Index of the last value used.</summary>
    internal int NextIndex { get; private set; }

    /// <summary>Index of the value subtracted next.</summary>
    internal int NextPIndex { get; private set; }

    /// <summary>Uniform value in [0, <paramref name="max"/>); the original <c>random(x)</c>.</summary>
    public float Next(float max) => (float)(Sample() * max);

    /// <summary>Uniform value in [<paramref name="min"/>, <paramref name="max"/>).</summary>
    public float Range(float min, float max) => min + Next(max - min);

    /// <summary>Uniform integer in [0, <paramref name="maxExclusive"/>).</summary>
    public int NextInt(int maxExclusive) => (int)(Sample() * maxExclusive);

    /// <summary>Overwrites the generator state with values read from a snapshot.</summary>
    internal void Restore(ReadOnlySpan<int> state, int nextIndex, int nextPIndex)
    {
        state.CopyTo(_state);
        NextIndex = nextIndex;
        NextPIndex = nextPIndex;
    }

    private double Sample()
    {
        var next = NextIndex + 1 >= StateLength ? 1 : NextIndex + 1;
        var nextP = NextPIndex + 1 >= StateLength ? 1 : NextPIndex + 1;
        var value = _state[next] - _state[nextP];
        if (value == int.MaxValue)
        {
            value--;
        }

        if (value < 0)
        {
            value += int.MaxValue;
        }

        _state[next] = value;
        NextIndex = next;
        NextPIndex = nextP;
        return value * (1.0 / int.MaxValue);
    }
}
