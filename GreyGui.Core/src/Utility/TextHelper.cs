using System.Runtime.InteropServices;
using Microsoft.Xna.Framework;

namespace GreyGui;

public static class TextHelper
{
    public static (UiVertex[], int[]) GenerateTextVertices(List<GlyphInfo> glyphInfoList, Color color, float fontSize)
    {
        UiVertex[] vertices = new UiVertex[glyphInfoList.Count * 4];
        int[] indices = new int[glyphInfoList.Count * 6];
        int indexCount = 0;

        int vertexCount = 0;
        float scale = fontSize / GreyGui.TextSystem.GlyphPixelSize;
        ReadOnlySpan<GlyphInfo> glyphInfoSpan = CollectionsMarshal.AsSpan(glyphInfoList);
        for (int i = 0; i < glyphInfoSpan.Length; ++i)
        {
            GlyphInfo glyphInfo = glyphInfoSpan[i];
            Vector2 finalSize = glyphInfo.SrcRect.Size.ToVector2() * scale;
            // rectParams.Z = fontSize to tell what the anti-aliasing distant value should be
            // rectParams.W = -1 tells the shader we are rendering text
            Vector4 rectParams = new(finalSize.X, finalSize.Y, fontSize, -1);

            // (float left, float top) = cursor - glyphInfo.Origin * scale;
            // float right = left + finalSize.X;
            // float bottom = top + finalSize.Y;

            float uvLeft = (float)glyphInfo.SrcRect.Left / GreyGui.Atlas.Width;
            float uvRight = (float)glyphInfo.SrcRect.Right / GreyGui.Atlas.Width;
            float uvTop = (float)glyphInfo.SrcRect.Top / GreyGui.Atlas.Height;
            float ubBottom = (float)glyphInfo.SrcRect.Bottom / GreyGui.Atlas.Height;

            indices[indexCount++] = vertexCount;
            indices[indexCount++] = vertexCount + 1;
            indices[indexCount++] = vertexCount + 2;
            indices[indexCount++] = vertexCount + 2;
            indices[indexCount++] = vertexCount + 3;
            indices[indexCount++] = vertexCount;

            vertices[vertexCount++] = new UiVertex() { Position = new(), BorderColor = color, Color = color, LocalCoord = new(0, 0), TexCoord = new(uvLeft, uvTop), RectParams = rectParams };
            vertices[vertexCount++] = new UiVertex() { Position = new(), BorderColor = color, Color = color, LocalCoord = new(1, 0), TexCoord = new(uvRight, uvTop), RectParams = rectParams };
            vertices[vertexCount++] = new UiVertex() { Position = new(), BorderColor = color, Color = color, LocalCoord = new(1, 1), TexCoord = new(uvRight, ubBottom), RectParams = rectParams };
            vertices[vertexCount++] = new UiVertex() { Position = new(), BorderColor = color, Color = color, LocalCoord = new(0, 1), TexCoord = new(uvLeft, ubBottom), RectParams = rectParams };

        }
        return (vertices, indices);
    }

    public static void UpdateUiVertex(UiVertex[] uiVertices, List<GlyphInfo> glyphInfoList, Vector2 position, Color color, float fontSize)
    {
        float scale = fontSize / GreyGui.TextSystem.GlyphPixelSize;
        ReadOnlySpan<GlyphInfo> glyphInfoSpan = CollectionsMarshal.AsSpan(glyphInfoList);
        Vector2 cursor = position;
        int vertexCount = 0;
        for (int i = 0; i < glyphInfoSpan.Length; ++i)
        {
            GlyphInfo glyphInfo = glyphInfoSpan[i];
            Vector2 finalSize = glyphInfo.SrcRect.Size.ToVector2() * scale;
            Vector4 rectParams = new(finalSize.X, finalSize.Y, fontSize, -1);

            (float left, float top) = cursor - glyphInfo.Origin * scale;
            float right = left + finalSize.X;
            float bottom = top + finalSize.Y;

            float uvLeft = (float)glyphInfo.SrcRect.Left / GreyGui.Atlas.Width;
            float uvRight = (float)glyphInfo.SrcRect.Right / GreyGui.Atlas.Width;
            float uvTop = (float)glyphInfo.SrcRect.Top / GreyGui.Atlas.Height;
            float ubBottom = (float)glyphInfo.SrcRect.Bottom / GreyGui.Atlas.Height;

            uiVertices[vertexCount++] = new UiVertex() { Position = new(left, top, 0), BorderColor = color, Color = color, LocalCoord = new(0, 0), TexCoord = new(uvLeft, uvTop), RectParams = rectParams };
            uiVertices[vertexCount++] = new UiVertex() { Position = new(right, top, 0), BorderColor = color, Color = color, LocalCoord = new(1, 0), TexCoord = new(uvRight, uvTop), RectParams = rectParams };
            uiVertices[vertexCount++] = new UiVertex() { Position = new(right, bottom, 0), BorderColor = color, Color = color, LocalCoord = new(1, 1), TexCoord = new(uvRight, ubBottom), RectParams = rectParams };
            uiVertices[vertexCount++] = new UiVertex() { Position = new(left, bottom, 0), BorderColor = color, Color = color, LocalCoord = new(0, 1), TexCoord = new(uvLeft, ubBottom), RectParams = rectParams };

            cursor.X += glyphInfo.AdvanceWidth * scale;
        }
    }
    public static (UiVertex[], int[]) GenerateTextVertices(string fontName, string text, Vector2 position, Color color, float fontSize)
    {
        UiVertex[] vertices = new UiVertex[text.Length * 4];
        int[] indices = new int[text.Length * 6];
        int indexCount = 0;

        int vertexCount = 0;
        float scale = fontSize / GreyGui.TextSystem.GlyphPixelSize;
        FontInfo fontInfo = GreyGui.TextSystem.GetFontInfo(fontName);
        Vector2 cursor = position;
        foreach (char c in text)
        {
            GlyphInfo glyphInfo = fontInfo.GlyphInfoMap[c];

            Vector2 finalSize = glyphInfo.SrcRect.Size.ToVector2() * scale;
            // rectParams.Z = fontSize to tell what the anti-aliasing distant value should be
            // rectParams.W = -1 tells the shader we are rendering text
            Vector4 rectParams = new(finalSize.X, finalSize.Y, fontSize, -1);

            (float left, float top) = cursor - glyphInfo.Origin * scale;
            float right = left + finalSize.X;
            float bottom = top + finalSize.Y;

            float uvLeft = (float)glyphInfo.SrcRect.Left / GreyGui.Atlas.Width;
            float uvRight = (float)glyphInfo.SrcRect.Right / GreyGui.Atlas.Width;
            float uvTop = (float)glyphInfo.SrcRect.Top / GreyGui.Atlas.Height;
            float ubBottom = (float)glyphInfo.SrcRect.Bottom / GreyGui.Atlas.Height;

            indices[indexCount++] = vertexCount;
            indices[indexCount++] = vertexCount + 1;
            indices[indexCount++] = vertexCount + 2;
            indices[indexCount++] = vertexCount + 2;
            indices[indexCount++] = vertexCount + 3;
            indices[indexCount++] = vertexCount;

            vertices[vertexCount++] = new UiVertex() { Position = new(left, top, 0), BorderColor = color, Color = color, LocalCoord = new(0, 0), TexCoord = new(uvLeft, uvTop), RectParams = rectParams };
            vertices[vertexCount++] = new UiVertex() { Position = new(right, top, 0), BorderColor = color, Color = color, LocalCoord = new(1, 0), TexCoord = new(uvRight, uvTop), RectParams = rectParams };
            vertices[vertexCount++] = new UiVertex() { Position = new(right, bottom, 0), BorderColor = color, Color = color, LocalCoord = new(1, 1), TexCoord = new(uvRight, ubBottom), RectParams = rectParams };
            vertices[vertexCount++] = new UiVertex() { Position = new(left, bottom, 0), BorderColor = color, Color = color, LocalCoord = new(0, 1), TexCoord = new(uvLeft, ubBottom), RectParams = rectParams };

            cursor.X += glyphInfo.AdvanceWidth * scale;
        }
        return (vertices, indices);
    }

}