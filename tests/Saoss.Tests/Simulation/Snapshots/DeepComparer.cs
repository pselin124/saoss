using System.Collections;
using System.Reflection;
using System.Runtime.CompilerServices;
using Saoss.Core.Simulation;

namespace Saoss.Tests.Simulation.Snapshots;

/// <summary>
/// Compares two object graphs field by field (public and private, recursively) and lists the differences.
/// Floats are compared bit for bit. Ship catalog data (Saoss.Core.Ships) must be the same instances.
/// BCL collections are compared as sequences, so capacity and version fields do not matter. Free pool slots are
/// equal whatever their stale fields hold, except a free beam's velocity, which the next shot reuses.
/// </summary>
internal static class DeepComparer
{
    private const int MaxDifferences = 20;
    private const string CatalogNamespace = "Saoss.Core.Ships";

    /// <summary>Returns the paths and values that differ, at most 20.</summary>
    public static IReadOnlyList<string> Differences(object? expected, object? actual)
    {
        var walker = new Walker();
        walker.Compare("root", expected, actual);
        return walker.Found;
    }

    /// <summary>Fails the test with the differences, if any.</summary>
    public static void AssertEqual(object? expected, object? actual, string context)
    {
        var differences = Differences(expected, actual);
        Assert.IsEmpty(differences, $"{context}:\n{string.Join("\n", differences)}");
    }

    private sealed class Walker
    {
        private readonly HashSet<(object, object)> _visited = new(PairComparer.Instance);

        public List<string> Found { get; } = [];

        public void Compare(string path, object? a, object? b)
        {
            if (Found.Count >= MaxDifferences || ReferenceEquals(a, b))
            {
                return;
            }

            if (a is null || b is null)
            {
                Report(path, a, b);
                return;
            }

            if (IsSequence(a) && IsSequence(b))
            {
                CompareSequences(path, (IEnumerable)a, (IEnumerable)b);
                return;
            }

            var type = a.GetType();
            if (type != b.GetType())
            {
                Report(path, type.Name, b.GetType().Name);
                return;
            }

            CompareValues(path, type, a, b);
        }

        // BCL and compiler-generated collections; the game's own types are walked field by field.
        private static bool IsSequence(object value) =>
            value is IEnumerable and not string && (value is Array || value.GetType().Namespace?.StartsWith("Saoss", StringComparison.Ordinal) != true);

        private void CompareValues(string path, Type type, object a, object b)
        {
            if (a is float fa)
            {
                CompareBits(path, BitConverter.SingleToInt32Bits(fa), BitConverter.SingleToInt32Bits((float)b), a, b);
            }
            else if (a is double da)
            {
                CompareBits(path, BitConverter.DoubleToInt64Bits(da), BitConverter.DoubleToInt64Bits((double)b), a, b);
            }
            else if (type.IsPrimitive || type.IsEnum || a is string or decimal)
            {
                if (!a.Equals(b))
                {
                    Report(path, a, b);
                }
            }
            else if (type.Namespace == CatalogNamespace || a is Delegate)
            {
                Report(path, "a different instance", "the shared catalog instance");
            }
            else if (!IsFreeSlotPair(path, a, b) && (type.IsValueType || _visited.Add((a, b))))
            {
                CompareFields(path, type, a, b);
            }
        }

        private void CompareBits(string path, long bitsA, long bitsB, object a, object b)
        {
            if (bitsA != bitsB)
            {
                Report(path, a, b);
            }
        }

        // Free pool slots only hold leftovers; a free beam's velocity is kept for its next use.
        private bool IsFreeSlotPair(string path, object a, object b)
        {
            switch (a, b)
            {
                case (Projectile pa, Projectile pb):
                    return !pa.IsActive && !pb.IsActive;
                case (Pickup pa, Pickup pb):
                    return !pa.IsActive && !pb.IsActive;
                case (Beam ba, Beam bb) when !ba.IsActive && !bb.IsActive:
                    Compare(path + ".VelocityX", ba.VelocityX, bb.VelocityX);
                    Compare(path + ".VelocityY", ba.VelocityY, bb.VelocityY);
                    return true;
                default:
                    return false;
            }
        }

        private void CompareFields(string path, Type type, object a, object b)
        {
            for (var current = type; current != null && current != typeof(object); current = current.BaseType)
            {
                var fields = current.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly);
                foreach (var field in fields)
                {
                    Compare($"{path}.{field.Name}", field.GetValue(a), field.GetValue(b));
                }
            }
        }

        private void CompareSequences(string path, IEnumerable a, IEnumerable b)
        {
            var listA = a.Cast<object?>().ToList();
            var listB = b.Cast<object?>().ToList();
            if (listA.Count != listB.Count)
            {
                Report(path + ".Count", listA.Count, listB.Count);
                return;
            }

            for (var i = 0; i < listA.Count; i++)
            {
                Compare($"{path}[{i}]", listA[i], listB[i]);
            }
        }

        private void Report(string path, object? a, object? b) => Found.Add($"{path}: {a ?? "null"} != {b ?? "null"}");
    }

    private sealed class PairComparer : IEqualityComparer<(object, object)>
    {
        public static readonly PairComparer Instance = new();

        public bool Equals((object, object) x, (object, object) y) => ReferenceEquals(x.Item1, y.Item1) && ReferenceEquals(x.Item2, y.Item2);

        public int GetHashCode((object, object) obj) => HashCode.Combine(RuntimeHelpers.GetHashCode(obj.Item1), RuntimeHelpers.GetHashCode(obj.Item2));
    }
}
