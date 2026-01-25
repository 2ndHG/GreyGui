using System.Numerics;
using System.Runtime.CompilerServices;

namespace SimpleSdf
{
    public static class MathHelper
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Cross(Vector2 a, Vector2 b) => a.X * b.Y - a.Y * b.X;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int NonZeroSign(float n) => n >= 0 ? 1 : -1;

        public static int SolveQuadratic(Span<float> x, float a, float b, float c)
        {
            if (MathF.Abs(a) < 1e-5f)
            {
                if (MathF.Abs(b) < 1e-5f) return 0;
                x[0] = -c / b;
                return 1;
            }
            float dscr = b * b - 4 * a * c;
            if (dscr > 0)
            {
                dscr = MathF.Sqrt(dscr);
                x[0] = (-b + dscr) / (2 * a);
                x[1] = (-b - dscr) / (2 * a);
                return 2;
            }
            else if (dscr == 0)
            {
                x[0] = -b / (2 * a);
                return 1;
            }
            return 0;
        }

        public static int SolveCubic(Span<float> x, float a, float b, float c, float d)
        {
            if (MathF.Abs(a) < 1e-5f)
            {
                return SolveQuadratic(x, b, c, d);
            }

            float bn = b / a;
            float cn = c / a;
            float dn = d / a;

            float bn3 = bn / 3.0f;
            float p = cn - 3 * bn3 * bn3;
            float q = dn + 2 * bn3 * bn3 * bn3 - cn * bn3;

            float p3 = p / 3.0f;
            float q2 = q / 2.0f;
            float discriminant = q2 * q2 + p3 * p3 * p3;

            if (discriminant < 0)
            {
                float r = MathF.Sqrt(-p3 * p3 * p3);
                float theta = MathF.Acos(-q2 / r);
                float alpha = 2.0f * MathF.Sqrt(-p3);

                x[0] = alpha * MathF.Cos(theta / 3.0f) - bn3;
                x[1] = alpha * MathF.Cos((theta + 2 * MathF.PI) / 3.0f) - bn3;
                x[2] = alpha * MathF.Cos((theta - 2 * MathF.PI) / 3.0f) - bn3;
                return 3;
            }
            else
            {
                float u1 = -q2 + MathF.Sqrt(discriminant);
                float u2 = -q2 - MathF.Sqrt(discriminant);

                float s1 = (u1 >= 0) ? MathF.Pow(u1, 1.0f / 3.0f) : -MathF.Pow(-u1, 1.0f / 3.0f);
                float s2 = (u2 >= 0) ? MathF.Pow(u2, 1.0f / 3.0f) : -MathF.Pow(-u2, 1.0f / 3.0f);

                x[0] = (s1 + s2) - bn3;

                if (MathF.Abs(s1 - s2) < 1e-5f)
                {
                    x[1] = -bn3 - (s1 + s2) / 2.0f;
                    return 2;
                }
                return 1;
            }
        }
    }


}