using System;
using System.Runtime.CompilerServices;

namespace SimpleSdf
{
    /// Represents a signed distance and alignment, which together can be compared to uniquely determine the closest edge segment.
    public struct SignedDistance
    {
        const float significantDistance = 1e-4f;
        public static readonly SignedDistance Infinite = new(float.MaxValue, 1f);

        public float Distance;
        public float Dot;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public SignedDistance(float dist, float d)
        {
            Distance = dist;
            Dot = d;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator <(SignedDistance a, SignedDistance b)
        {
            float absA = MathF.Abs(a.Distance);
            float absB = MathF.Abs(b.Distance);
            if (MathF.Abs(absA - absB) < significantDistance)
                return a.Dot < b.Dot;
            return absA < absB;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator >(SignedDistance a, SignedDistance b)
        {
            return Math.Abs(a.Distance) > Math.Abs(b.Distance) ||
                   Math.Abs(a.Distance) == Math.Abs(b.Distance) && a.Dot > b.Dot;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator <=(SignedDistance a, SignedDistance b)
        {
            return Math.Abs(a.Distance) < Math.Abs(b.Distance) ||
                   Math.Abs(a.Distance) == Math.Abs(b.Distance) && a.Dot <= b.Dot;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator >=(SignedDistance a, SignedDistance b)
        {
            return Math.Abs(a.Distance) > Math.Abs(b.Distance) ||
                   Math.Abs(a.Distance) == Math.Abs(b.Distance) && a.Dot >= b.Dot;
        }
    }
}