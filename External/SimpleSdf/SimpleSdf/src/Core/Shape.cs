
using System.Numerics;
using Typography.OpenFont;

namespace SimpleSdf;

public class Shape
{
    public List<List<EdgeSegment>> contourList { get; set; } = []; // a contour contains a edge list
    public Shape(Glyph glyph)
    {
        if (!glyph.TtfWoffInfo.HasValue)
        {
            return;
        }
        ushort[] ends = glyph.TtfWoffInfo.Value.endPoints;
        GlyphPointF[] glyphPoints = glyph.TtfWoffInfo.Value.glyphPoints;
        int start = 0;

        foreach (ushort end in ends)
        {
            int count = end - start + 1;
            int offset = 0;
            List<EdgeSegment> currentContour = [];
            contourList.Add(currentContour);
            while (offset < count && !glyphPoints[start + offset].OnCurve)
                offset++;

            if (offset >= count) return;

            int firstOnCurveIndex = start + offset;
            Vector2 startP = FlipY(glyphPoints[firstOnCurveIndex].Point);
            Vector2 currP = startP;

            for (int i = 1; i <= count; i++)
            {
                int nextIdx = start + ((offset + i) % count);
                GlyphPointF nextPtData = glyphPoints[nextIdx];
                Vector2 nextP = FlipY(glyphPoints[nextIdx].Point);

                if (nextPtData.OnCurve)
                {
                    currentContour.Add(new LinearSegment(currP, nextP));
                    currP = nextP;
                }
                else
                {
                    int nextNextIdx = start + ((offset + i + 1) % count);
                    GlyphPointF nextNextPtData = glyphPoints[nextNextIdx];
                    Vector2 nextNextP = FlipY(glyphPoints[nextNextIdx].Point);

                    if (nextNextPtData.OnCurve)
                    {
                        currentContour.Add(new QuadraticSegment(currP, nextP, nextNextP));
                        currP = nextNextP;
                        i++;
                    }
                    else
                    {
                        Vector2 midP = (nextP + nextNextP) * 0.5f;
                        currentContour.Add(new QuadraticSegment(currP, nextP, midP));
                        currP = midP;
                    }
                }
            }
            start = end + 1;
        }
    }

    private static Vector2 FlipY(Vector2 vector2)
    {
        return new Vector2(vector2.X, -vector2.Y);
    }
}
