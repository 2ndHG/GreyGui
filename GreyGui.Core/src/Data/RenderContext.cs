using System.Runtime.InteropServices;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GreyGui;

public class RenderContext
{
    const int MAX_VERTEX_COUNT = 2048;
    const int MAX_INDEX_COUNT = 2048;
    public UiVertex[] Vertices => _vertices;
    public int[] Indices => _indices;
    public int VertexCount { get; private set; } = 0;
    public int IndexCount { get; private set; } = 0;
    public readonly List<DrawBatch> Batches = [new DrawBatch()];

    private UiVertex[] _vertices = new UiVertex[MAX_VERTEX_COUNT];
    private int[] _indices = new int[MAX_INDEX_COUNT];
    public double ElapsedTimeSecond { get; set; }

    public void Clear()
    {
        VertexCount = 0;
        IndexCount = 0;
        Batches.Clear();
        Batches.Add(new DrawBatch());
    }

    public void AddCommand_Unstable(Texture2D texture, UiVertex[] vertices, int[] indices, Rectangle scissor)
    {
        // full
        EnsureCapacity(vertices.Length, indices.Length);

        PrepareDrawBatchForTexture(texture, scissor);
        AddIndicesToLastBatch(indices.Length);

        int offset = VertexCount;
        Array.Copy(vertices, 0, _vertices, VertexCount, vertices.Length);
        VertexCount += vertices.Length;

        for (int i = 0; i < indices.Length; i++)
        {
            _indices[IndexCount + i] = indices[i] + offset;
        }
        IndexCount += indices.Length;
    }

    public void FillRect(Rectangle dest, Color color, Color borderColor, float borderRadius, float borderWidth, Rectangle scissor)
    {
        EnsureCapacity(4, 6);

        PrepareDrawBatchForTexture(GreyGui.Atlas, scissor);
        AddIndicesToLastBatch(6);

        Vector4 rectParams = new(dest.Width, dest.Height, borderRadius, borderWidth);
        int vOffset = VertexCount;

        SetVertex(VertexCount++, new Vector3(dest.Left, dest.Top, 0), color, borderColor, GreyGui.AtlasPixelUv, new Vector2(0, 0), rectParams);
        SetVertex(VertexCount++, new Vector3(dest.Right, dest.Top, 0), color, borderColor, GreyGui.AtlasPixelUv, new Vector2(1, 0), rectParams);
        SetVertex(VertexCount++, new Vector3(dest.Right, dest.Bottom, 0), color, borderColor, GreyGui.AtlasPixelUv, new Vector2(1, 1), rectParams);
        SetVertex(VertexCount++, new Vector3(dest.Left, dest.Bottom, 0), color, borderColor, GreyGui.AtlasPixelUv, new Vector2(0, 1), rectParams);

        _indices[IndexCount++] = vOffset + 0;
        _indices[IndexCount++] = vOffset + 1;
        _indices[IndexCount++] = vOffset + 2;
        _indices[IndexCount++] = vOffset + 2;
        _indices[IndexCount++] = vOffset + 3;
        _indices[IndexCount++] = vOffset + 0;
    }

    public void FillRect(Rectangle dest, Color colorTl, Color colorTr, Color colorBl, Color colorBr, Color borderColorTl, Color borderColorTr, Color borderColorBl, Color borderColorBr, float borderRadius, float borderWidth, Rectangle scissor)
    {
        EnsureCapacity(4, 6);
        PrepareDrawBatchForTexture(GreyGui.Atlas, scissor);
        AddIndicesToLastBatch(6);

        Vector4 rectParams = new(dest.Width, dest.Height, borderRadius, borderWidth);
        int vOffset = VertexCount;

        SetVertex(VertexCount++, new Vector3(dest.Left, dest.Top, 0), colorTl, borderColorTl, GreyGui.AtlasPixelUv, new Vector2(0, 0), rectParams);
        SetVertex(VertexCount++, new Vector3(dest.Right, dest.Top, 0), colorTr, borderColorTr, GreyGui.AtlasPixelUv, new Vector2(1, 0), rectParams);
        SetVertex(VertexCount++, new Vector3(dest.Right, dest.Bottom, 0), colorBr, borderColorBr, GreyGui.AtlasPixelUv, new Vector2(1, 1), rectParams);
        SetVertex(VertexCount++, new Vector3(dest.Left, dest.Bottom, 0), colorBl, borderColorBl, GreyGui.AtlasPixelUv, new Vector2(0, 1), rectParams);

        _indices[IndexCount++] = vOffset + 0;
        _indices[IndexCount++] = vOffset + 1;
        _indices[IndexCount++] = vOffset + 2;
        _indices[IndexCount++] = vOffset + 2;
        _indices[IndexCount++] = vOffset + 3;
        _indices[IndexCount++] = vOffset + 0;
    }

    public void RenderTexture(Texture2D texture, Rectangle destRect, Rectangle srcRect, Color color, Color borderColor, float borderRadius, float borderWidth, Rectangle scissor)
    {
        EnsureCapacity(4, 6);

        PrepareDrawBatchForTexture(texture, scissor);
        AddIndicesToLastBatch(6);


        Vector4 rectParams = new(destRect.Width, destRect.Height, borderRadius, borderWidth);
        int vOffset = VertexCount;

        float left = srcRect.Left / (float)texture.Width;
        float right = srcRect.Right / (float)texture.Width;
        float top = srcRect.Top / (float)texture.Height;
        float bottom = srcRect.Bottom / (float)texture.Height;

        SetVertex(VertexCount++, new Vector3(destRect.Left, destRect.Top, 0), color, borderColor, new Vector2(left, top), new Vector2(0, 0), rectParams);
        SetVertex(VertexCount++, new Vector3(destRect.Right, destRect.Top, 0), color, borderColor, new Vector2(right, top), new Vector2(1, 0), rectParams);
        SetVertex(VertexCount++, new Vector3(destRect.Right, destRect.Bottom, 0), color, borderColor, new Vector2(right, bottom), new Vector2(1, 1), rectParams);
        SetVertex(VertexCount++, new Vector3(destRect.Left, destRect.Bottom, 0), color, borderColor, new(left, bottom), new Vector2(0, 1), rectParams);

        _indices[IndexCount++] = vOffset + 0;
        _indices[IndexCount++] = vOffset + 1;
        _indices[IndexCount++] = vOffset + 2;
        _indices[IndexCount++] = vOffset + 2;
        _indices[IndexCount++] = vOffset + 3;
        _indices[IndexCount++] = vOffset + 0;
    }

    public Span<UiVertex> RequireRectVertices(Texture2D texture, Rectangle destRect, Rectangle srcRect, Color color, Color borderColor, float borderRadius, float borderWidth, Rectangle scissor)
    {
        EnsureCapacity(4, 6);

        PrepareDrawBatchForTexture(texture, scissor);
        AddIndicesToLastBatch(6);

        Vector4 rectParams = new(destRect.Width, destRect.Height, borderRadius, borderWidth);
        int vOffset = VertexCount;

        float left = srcRect.Left / (float)texture.Width;
        float right = srcRect.Right / (float)texture.Width;
        float top = srcRect.Top / (float)texture.Height;
        float bottom = srcRect.Bottom / (float)texture.Height;

        SetVertex(VertexCount++, new Vector3(destRect.Left, destRect.Top, 0), color, borderColor, new Vector2(left, top), new Vector2(0, 0), rectParams);
        SetVertex(VertexCount++, new Vector3(destRect.Right, destRect.Top, 0), color, borderColor, new Vector2(right, top), new Vector2(1, 0), rectParams);
        SetVertex(VertexCount++, new Vector3(destRect.Right, destRect.Bottom, 0), color, borderColor, new Vector2(right, bottom), new Vector2(1, 1), rectParams);
        SetVertex(VertexCount++, new Vector3(destRect.Left, destRect.Bottom, 0), color, borderColor, new(left, bottom), new Vector2(0, 1), rectParams);

        _indices[IndexCount++] = vOffset + 0;
        _indices[IndexCount++] = vOffset + 1;
        _indices[IndexCount++] = vOffset + 2;
        _indices[IndexCount++] = vOffset + 2;
        _indices[IndexCount++] = vOffset + 3;
        _indices[IndexCount++] = vOffset + 0;
        return new Span<UiVertex>(_vertices, vOffset, 4);
    }

    public Span<UiVertex> GetLastAddedVertices(int count)
    {
        if (count > VertexCount)
        {
            throw new ArgumentOutOfRangeException(nameof(count), "Requested vertex count exceeds the number of vertices in the current batch.");
        }
        return new Span<UiVertex>(_vertices, VertexCount - count, count);
    }

    public void RenderTextUsingCharIndices(List<int> indices, int startIndex, int length, Vector2 position, float fontSize, Color color, Rectangle scissor, out AddVertexResult result)
    {
        result = new AddVertexResult() { VertexStart = VertexCount, VertexCount = length * 4, IndexStart = IndexCount, IndexCount = length * 6 };
        RenderTextUsingCharIndices(indices, startIndex, length, position, fontSize, color, scissor);
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="indices">List of rendering characters indices in GreyGui.TextSystem.GlyphInfoList</param>
    /// <param name="startIndex">Starting index of indices</param>
    /// <param name="length">Length</param>
    /// <param name="position">Position of the first character</param>
    /// <param name="fontSize">Font size</param>
    /// <param name="color">Text color</param>
    /// <param name="scissor">Screen scissor</param>
    public void RenderTextUsingCharIndices(List<int> indices, int startIndex, int length, Vector2 position, float fontSize, Color color, Rectangle scissor)
    {
        EnsureCapacity(4 * length, 6 * length);
        PrepareDrawBatchForTexture(GreyGui.Atlas, scissor);
        AddIndicesToLastBatch(6 * length);

        float scale = fontSize / GreyGui.TextSystem.GlyphPixelSize;
        Vector2 cursor = position;
        length += startIndex;
        for (int i = startIndex; i < length; ++i)
        {
            int vOffset = VertexCount;

            GlyphInfo glyphInfo = GreyGui.TextSystem.GlyphInfoList[indices[i]];
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

            SetVertex(VertexCount++, new Vector3(left, top, 0), color, color, new(uvLeft, uvTop), new(0, 0), rectParams);
            SetVertex(VertexCount++, new Vector3(right, top, 0), color, color, new(uvRight, uvTop), new(1, 0), rectParams);
            SetVertex(VertexCount++, new Vector3(right, bottom, 0), color, color, new(uvRight, ubBottom), new(1, 1), rectParams);
            SetVertex(VertexCount++, new Vector3(left, bottom, 0), color, color, new(uvLeft, ubBottom), new(0, 1), rectParams);

            _indices[IndexCount++] = vOffset + 0;
            _indices[IndexCount++] = vOffset + 1;
            _indices[IndexCount++] = vOffset + 2;
            _indices[IndexCount++] = vOffset + 2;
            _indices[IndexCount++] = vOffset + 3;
            _indices[IndexCount++] = vOffset + 0;
            cursor.X += glyphInfo.AdvanceWidth * scale;
        }
    }
    public void RenderText(List<GlyphInfo> glyphInfoList, Vector2 position, float fontSize, Color color, Rectangle scissor)
    {
        RenderText(CollectionsMarshal.AsSpan(glyphInfoList), 0, glyphInfoList.Count, position, fontSize, color, scissor);
    }
    public void RenderText(ReadOnlySpan<GlyphInfo> glyphInfoSpan, int startIndex, int length, Vector2 position, float fontSize, Color color, Rectangle scissor)
    {
        EnsureCapacity(4 * length, 6 * length);
        PrepareDrawBatchForTexture(GreyGui.Atlas, scissor);
        AddIndicesToLastBatch(6 * length);

        float scale = fontSize / GreyGui.TextSystem.GlyphPixelSize;
        Vector2 cursor = position;
        length += startIndex;
        for (int i = startIndex; i < length; ++i)
        {
            int vOffset = VertexCount;

            GlyphInfo glyphInfo = glyphInfoSpan[i];
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

            SetVertex(VertexCount++, new Vector3(left, top, 0), color, color, new(uvLeft, uvTop), new(0, 0), rectParams);
            SetVertex(VertexCount++, new Vector3(right, top, 0), color, color, new(uvRight, uvTop), new(1, 0), rectParams);
            SetVertex(VertexCount++, new Vector3(right, bottom, 0), color, color, new(uvRight, ubBottom), new(1, 1), rectParams);
            SetVertex(VertexCount++, new Vector3(left, bottom, 0), color, color, new(uvLeft, ubBottom), new(0, 1), rectParams);

            _indices[IndexCount++] = vOffset + 0;
            _indices[IndexCount++] = vOffset + 1;
            _indices[IndexCount++] = vOffset + 2;
            _indices[IndexCount++] = vOffset + 2;
            _indices[IndexCount++] = vOffset + 3;
            _indices[IndexCount++] = vOffset + 0;
            cursor.X += glyphInfo.AdvanceWidth * scale;
        }

    }

    public void FillCircle(Vector2 center, float radius, Color colorTl, Color colorTr, Color colorBl, Color colorBr, Color borderColorTl, Color borderColorTr, Color borderColorBl, Color borderColorBr, float borderWidth, Rectangle scissor)
    {
        EnsureCapacity(4, 6);
        PrepareDrawBatchForTexture(GreyGui.Atlas, scissor);
        AddIndicesToLastBatch(6);

        float left = center.X - radius;
        float right = center.X + radius;
        float top = center.Y - radius;
        float bottom = center.Y + radius;
        Vector4 rectParams = new(right - left, bottom - top, radius, borderWidth);
        int vOffset = VertexCount;

        SetVertex(VertexCount++, new Vector3(left, top, 0), colorTl, borderColorTl, GreyGui.AtlasPixelUv, new Vector2(0, 0), rectParams);
        SetVertex(VertexCount++, new Vector3(right, top, 0), colorTr, borderColorTr, GreyGui.AtlasPixelUv, new Vector2(1, 0), rectParams);
        SetVertex(VertexCount++, new Vector3(right, bottom, 0), colorBr, borderColorBr, GreyGui.AtlasPixelUv, new Vector2(1, 1), rectParams);
        SetVertex(VertexCount++, new Vector3(left, bottom, 0), colorBl, borderColorBl, GreyGui.AtlasPixelUv, new Vector2(0, 1), rectParams);

        _indices[IndexCount++] = vOffset + 0;
        _indices[IndexCount++] = vOffset + 1;
        _indices[IndexCount++] = vOffset + 2;
        _indices[IndexCount++] = vOffset + 2;
        _indices[IndexCount++] = vOffset + 3;
        _indices[IndexCount++] = vOffset + 0;
    }

    public void FillCircle(Vector2 center, float radius, Color color, Color borderColor, float borderWidth, Rectangle scissor)
    {
        EnsureCapacity(4, 6);
        PrepareDrawBatchForTexture(GreyGui.Atlas, scissor);
        AddIndicesToLastBatch(6);

        float left = center.X - radius;
        float right = center.X + radius;
        float top = center.Y - radius;
        float bottom = center.Y + radius;
        Vector4 rectParams = new(right - left, bottom - top, radius, borderWidth);
        int vOffset = VertexCount;

        SetVertex(VertexCount++, new Vector3(left, top, 0), color, borderColor, GreyGui.AtlasPixelUv, new Vector2(0, 0), rectParams);
        SetVertex(VertexCount++, new Vector3(right, top, 0), color, borderColor, GreyGui.AtlasPixelUv, new Vector2(1, 0), rectParams);
        SetVertex(VertexCount++, new Vector3(right, bottom, 0), color, borderColor, GreyGui.AtlasPixelUv, new Vector2(1, 1), rectParams);
        SetVertex(VertexCount++, new Vector3(left, bottom, 0), color, borderColor, GreyGui.AtlasPixelUv, new Vector2(0, 1), rectParams);

        _indices[IndexCount++] = vOffset + 0;
        _indices[IndexCount++] = vOffset + 1;
        _indices[IndexCount++] = vOffset + 2;
        _indices[IndexCount++] = vOffset + 2;
        _indices[IndexCount++] = vOffset + 3;
        _indices[IndexCount++] = vOffset + 0;
    }
    

    public void SetVertex(int index, Vector3 pos, Color col, Color borderCol, Vector2 uv, Vector2 local, Vector4 rParams)
    {
        _vertices[index].Position = pos;
        _vertices[index].Color = col;
        _vertices[index].BorderColor = borderCol;
        _vertices[index].TexCoord = uv;
        _vertices[index].LocalCoord = local;
        _vertices[index].RectParams = rParams;
    }

    public void EnsureCapacity(int newVertexCount, int newIndexCount)
    {
        if (VertexCount + newVertexCount > _vertices.Length)
        {
            int newCapacity = Math.Max(_vertices.Length * 2, newVertexCount);
            Array.Resize(ref _vertices, newCapacity);
        }
        if (IndexCount + newIndexCount > _indices.Length)
        {
            int newCapacity = Math.Max(_indices.Length * 2, newIndexCount);
            Array.Resize(ref _indices, newCapacity);
        }
    }

    public void PrepareDrawBatchForTexture(Texture2D incomingTexture, in Rectangle incomingScissor)
    {
        DrawBatch lastBatch = Batches[^1];
        if (
            incomingTexture != lastBatch.Texture ||
            incomingScissor != lastBatch.Scissor)
        {
            Batches.Add(new DrawBatch
            {
                Texture = incomingTexture,
                Scissor = incomingScissor,
                IndexOffset = IndexCount,
                IndexCount = 0
            });
        }
    }

    public void AddIndicesToLastBatch(int count)
    {
        CollectionsMarshal.AsSpan(Batches)[^1].IndexCount += count;
    }
}