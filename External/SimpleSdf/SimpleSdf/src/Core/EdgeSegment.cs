using System.Numerics;

namespace SimpleSdf;

public abstract class EdgeSegment
{
    public abstract SignedDistance SignedDistance(Vector2 origin);
    public abstract int ScanlineIntersections(Span<float> x, Span<int> dy, float y);
}

public class QuadraticSegment : EdgeSegment
{
    public Vector2 P0;
    public Vector2 P1;
    public Vector2 P2;
    public QuadraticSegment(Vector2 p0, Vector2 p1, Vector2 p2)
    {
        P0 = p0;
        P1 = p1;
        P2 = p2;
    }
    public override SignedDistance SignedDistance(Vector2 origin)
    {
        Vector2 qa = P0 - origin;
        Vector2 ab = P1 - P0;
        Vector2 br = P2 - P1 - ab;

        float a = Vector2.Dot(br, br);
        float b = 3 * Vector2.Dot(ab, br);
        float c = (2 * Vector2.Dot(ab, ab)) + Vector2.Dot(qa, br);
        float d = Vector2.Dot(qa, ab);

        Span<float> t = stackalloc float[3];

        int solutions = MathHelper.SolveCubic(t, a, b, c, d);

        float minDistance = MathHelper.NonZeroSign(MathHelper.Cross(P1 - P0, qa)) * qa.Length();
        float param = -Vector2.Dot(qa, P1 - P0) / Vector2.Dot(P1 - P0, P1 - P0);
        if (float.IsNaN(param)) param = 0;

        Vector2 epDir = P2 - P1;
        float distB = (P2 - origin).Length();
        if (distB < MathF.Abs(minDistance))
        {
            minDistance = MathHelper.NonZeroSign(MathHelper.Cross(epDir, P2 - origin)) * distB;
            param = Vector2.Dot(origin - P1, epDir) / Vector2.Dot(epDir, epDir);
        }

        for (int i = 0; i < solutions; i++)
        {
            if (t[i] > 0 && t[i] < 1)
            {
                Vector2 qe = qa + 2 * t[i] * ab + t[i] * t[i] * br;
                float dist = qe.Length();
                if (dist < MathF.Abs(minDistance))
                {
                    minDistance = MathHelper.NonZeroSign(MathHelper.Cross(ab + t[i] * br, qe)) * dist;
                    param = t[i];
                }
            }
        }

        if (param >= 0 && param <= 1)
            return new SignedDistance { Distance = minDistance, Dot = 0 };

        Vector2 dir = (param < 0.5f) ? (P1 - P0) : (P2 - P1);
        Vector2 pointVec = (param < 0.5f) ? qa : (P2 - origin);
        if (dir == Vector2.Zero) dir = Vector2.One;
        if (pointVec == Vector2.Zero) pointVec = Vector2.One;

        return new SignedDistance
        {
            Distance = minDistance,
            Dot = MathF.Abs(Vector2.Dot(Vector2.Normalize(dir), Vector2.Normalize(pointVec)))
        };
    }

    public override int ScanlineIntersections(Span<float> x, Span<int> dy, float y)
    {
        int total = 0;
        int nextDY = y > P0.Y ? 1 : -1;
        x[total] = P0.X;

        if (P0.Y == y)
        {
            if (P0.Y < P1.Y || (P0.Y == P1.Y && P0.Y < P2.Y))
                dy[total++] = 1;
            else
                nextDY = 1;
        }

        Vector2 ab = P1 - P0;
        Vector2 br = P2 - P1 - ab;
        Span<float> t = stackalloc float[2];
        int solutions = MathHelper.SolveQuadratic(t, br.Y, 2 * ab.Y, P0.Y - y);

        if (solutions >= 2 && t[0] > t[1]) { float temp = t[0]; t[0] = t[1]; t[1] = temp; }

        for (int i = 0; i < solutions && total < 2; ++i)
        {
            if (t[i] >= 0 && t[i] <= 1)
            {
                x[total] = P0.X + 2 * t[i] * ab.X + t[i] * t[i] * br.X;
                if (nextDY * (ab.Y + t[i] * br.Y) >= 0)
                {
                    dy[total++] = nextDY;
                    nextDY = -nextDY;
                }
            }
        }

        if (P2.Y == y)
        {
            if (nextDY > 0 && total > 0)
            {
                --total;
                nextDY = -1;
            }
            if ((P2.Y < P1.Y || (P2.Y == P1.Y && P2.Y < P0.Y)) && total < 2)
            {
                x[total] = P2.X;
                if (nextDY < 0)
                {
                    dy[total++] = -1;
                    nextDY = 1;
                }
            }
        }
        if (nextDY != (y >= P2.Y ? 1 : -1))
        {
            if (total > 0) --total;
            else
            {
                if (MathF.Abs(P2.Y - y) < MathF.Abs(P0.Y - y)) x[total] = P2.X;
                dy[total++] = nextDY;
            }
        }
        return total;
    }

}

public class LinearSegment: EdgeSegment
{
    public Vector2 P0;
    public Vector2 P1;
    public LinearSegment(Vector2 p0, Vector2 p1)
    {
        P0 = p0;
        P1 = p1;
    }
    public override int ScanlineIntersections(Span<float> x, Span<int> dy, float y)
    {
        if ((y >= P0.Y && y < P1.Y) || (y >= P1.Y && y < P0.Y))
        {
            float param = (y - P0.Y) / (P1.Y - P0.Y);
            x[0] = P0.X + (P1.X - P0.X) * param;
            dy[0] = Math.Sign(P1.Y - P0.Y);
            return 1;
        }
        return 0;
    }

    public override SignedDistance SignedDistance(Vector2 origin)
    {
        Vector2 aq = origin - P0;
        Vector2 ab = P1 - P0;
        float abLenSq = Vector2.Dot(ab, ab);
        float param;

        if (abLenSq < 1e-12f)
        {
            param = 0;
            return new SignedDistance { Distance = aq.Length(), Dot = 1.0f };
        }

        param = Vector2.Dot(aq, ab) / abLenSq;
        Vector2 eq = (param > 0.5f) ? P1 - origin : P0 - origin;
        float endpointDist = eq.Length();

        if (param > 0 && param < 1)
        {
            float orthoDist = MathHelper.Cross(aq, ab) / MathF.Sqrt(abLenSq);
            return new SignedDistance { Distance = orthoDist, Dot = 0 };
        }

        float cross = MathHelper.Cross(aq, ab);
        float sign = MathHelper.NonZeroSign(cross);
        float pseudoDist = MathF.Abs(Vector2.Dot(ab, eq)) / (MathF.Sqrt(abLenSq) * endpointDist);

        return new SignedDistance { Distance = sign * endpointDist, Dot = pseudoDist };
    }
}