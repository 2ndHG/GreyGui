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
                            SignedDistance sd = edge.SignedDistance(p);
                            
                            if (sd < absMinSd)
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
    }
}