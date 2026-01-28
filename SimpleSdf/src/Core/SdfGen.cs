using System;
using System.Numerics;

namespace SimpleSdf
{
    public static class SdfGenerator
    {
        public static float[] Generate(
            Shape shape,
            int width,
            int height,
            float scale,
            float translateX,
            float translateY,
            float range)
        {
            float[] output = new float[width * height];
            Span<float> xIntersections = stackalloc float[3];
            Span<int> dyDirections = stackalloc int[3];

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    Vector2 pixelPos = new Vector2(x + 0.5f, y + 0.5f);
                    Vector2 p = (pixelPos - new Vector2(translateX, translateY)) / scale;

                    SignedDistance absMinSd = SignedDistance.Infinite;
                    int winding = 0;

                    foreach (List<EdgeSegment> contour in shape.contourList)
                    {
                        foreach (EdgeSegment edge in contour)
                        {
                            SignedDistance sd = edge.SignedDistance(p, out float t);

                            // update if it's valid edge
                            if (sd < absMinSd && IsEdgeValid(shape, edge, t))
                            {
                                absMinSd = sd;
                            }

                            int hits = edge.ScanlineIntersections(xIntersections, dyDirections, p.Y);
                            for (int i = 0; i < hits; i++)
                            {
                                if (xIntersections[i] > p.X)
                                {
                                    winding += dyDirections[i];
                                }
                            }
                        }
                    }

                    float finalDist = absMinSd.Distance;

                    if (winding != 0)
                    {
                        finalDist = MathF.Abs(finalDist);
                    }
                    else
                    {
                        finalDist = -MathF.Abs(finalDist);
                    }

                    float val = ((finalDist * scale) / range) + 0.5f;
                    val = Math.Clamp(val, 0.0f, 1.0f);

                    output[y * width + x] = val;
                }
            }

            return output;
        }

        private static bool IsEdgeValid(Shape shape, EdgeSegment edge, float t)
        {
            Vector2 pEdge = edge.Point(t);
            Vector2 dir = edge.Direction(t);

            Vector2 normal = new Vector2(-dir.Y, dir.X);
            float len = normal.Length();
            if (len < 1e-6f) return true;
            normal /= len;

            float epsilon = 0.1f;
            Vector2 p1 = pEdge + normal * epsilon;
            Vector2 p2 = pEdge - normal * epsilon;

            int w1 = GetGlobalWinding(shape, p1);
            int w2 = GetGlobalWinding(shape, p2);

            if (w1 != 0 && w2 != 0)
            {
                return false;
            }

            return true;
        }

        private static int GetGlobalWinding(Shape shape, Vector2 p)
        {
            int winding = 0;
            Span<float> xIntersections = stackalloc float[3];
            Span<int> dyDirections = stackalloc int[3];

            foreach (var contour in shape.contourList)
            {
                foreach (var edge in contour)
                {
                    int hits = edge.ScanlineIntersections(xIntersections, dyDirections, p.Y);
                    for (int i = 0; i < hits; i++)
                    {
                        if (xIntersections[i] > p.X)
                        {
                            winding += dyDirections[i];
                        }
                    }
                }
            }
            return winding;
        }
    }
}