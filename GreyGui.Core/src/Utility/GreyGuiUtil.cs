using Microsoft.Xna.Framework;

namespace GreyGui;

public static class GreyGuiUtil
{
    public static Span<UiVertex> RotateVertices(this Span<UiVertex> vertices, float angle, Vector2 center)
    {
        if (vertices.Length == 0 || angle == 0f) return vertices;

        float cos = MathF.Cos(angle);
        float sin = MathF.Sin(angle);

        for (int i = 0; i < vertices.Length; i++)
        {
            ref UiVertex vertex = ref vertices[i];

            float x = vertex.Position.X - center.X;
            float y = vertex.Position.Y - center.Y;

            float newX = x * cos - y * sin;
            float newY = x * sin + y * cos;

            vertex.Position.X = newX + center.X;
            vertex.Position.Y = newY + center.Y;
        }
        return vertices;
    }

    public static Span<UiVertex> RotateRect(this Span<UiVertex> vertices, float angle)
    {
        if (vertices.Length == 0) return vertices;

        Vector2 min = new (vertices[0].Position.X, vertices[0].Position.Y);
        Vector2 max = min;

        for (int i = 1; i < vertices.Length; i++)
        {
            min.X = MathF.Min(min.X, vertices[i].Position.X);
            min.Y = MathF.Min(min.Y, vertices[i].Position.Y);
            max.X = MathF.Max(max.X, vertices[i].Position.X);
            max.Y = MathF.Max(max.Y, vertices[i].Position.Y);
        }

        Vector2 center = (min + max) / 2f;
        
        RotateVertices(vertices, angle, center);
        return vertices;
    }
}