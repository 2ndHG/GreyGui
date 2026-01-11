using GreyGui.Core;
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

    public void Clear()
    {
        VertexCount = 0;
        IndexCount = 0;
        Batches.Clear();
        Batches.Add(new DrawBatch());
    }

    public void AddCommand(Texture2D texture, UiVertex[] vertices, int[] indices, Rectangle scissor)
    {
        // full
        EnsureCapacity(vertices.Length, indices.Length);

        DrawBatch lastBatch = Batches[^1];
        if (scissor != lastBatch.Scissor || texture != lastBatch.Texture)
        {
            Batches.Add(new DrawBatch()
            {
                Texture = texture,
                IndexCount = 0,
                IndexOffset = IndexCount,
                Scissor = scissor
            });
            lastBatch = Batches[^1];
        }
        lastBatch.IndexCount += indices.Length;
        Batches[^1] = lastBatch;

        int offset = VertexCount;
        Array.Copy(vertices, 0, _vertices, VertexCount, vertices.Length);
        VertexCount += vertices.Length;

        for (int i = 0; i < indices.Length; i++)
        {
            _indices[IndexCount + i] = indices[i] + offset;
        }
        IndexCount += indices.Length;
    }

    public void FillRect(Rectangle dest, Color color, Color borderColor, float borderRadius, float borderWidth, Texture2D texture, Rectangle scissor)
    {
        EnsureCapacity(4, 6);

        DrawBatch lastBatch = Batches[^1];
        if (
            texture != lastBatch.Texture ||
            scissor != lastBatch.Scissor)
        {
            Batches.Add(new DrawBatch
            {
                Texture = texture,
                Scissor = scissor,
                IndexOffset = IndexCount,
                IndexCount = 0
            });
            lastBatch = Batches[^1];
        }
        lastBatch.IndexCount += 6;
        Batches[^1] = lastBatch;

        Vector4 rectParams = new(dest.Width, dest.Height, borderRadius, borderWidth);
        int vOffset = VertexCount;
        SetVertex(VertexCount++, new Vector3(dest.Left, dest.Top, 0), color, borderColor, GreyGui.PixelUV, new Vector2(0, 0), rectParams);
        SetVertex(VertexCount++, new Vector3(dest.Right, dest.Top, 0), color, borderColor, GreyGui.PixelUV, new Vector2(1, 0), rectParams);
        SetVertex(VertexCount++, new Vector3(dest.Right, dest.Bottom, 0), color, borderColor, GreyGui.PixelUV, new Vector2(1, 1), rectParams);
        SetVertex(VertexCount++, new Vector3(dest.Left, dest.Bottom, 0), color, borderColor, GreyGui.PixelUV, new Vector2(0, 1), rectParams);

        _indices[IndexCount++] = vOffset + 0;
        _indices[IndexCount++] = vOffset + 1;
        _indices[IndexCount++] = vOffset + 2;
        _indices[IndexCount++] = vOffset + 2;
        _indices[IndexCount++] = vOffset + 3;
        _indices[IndexCount++] = vOffset + 0;
    }

    public void RenderText(Point position, string text, float fontSize)
    {
        if (string.IsNullOrEmpty(text))
            return;

        Vector2 cursor = new(position.X, position.Y);
        float invW = 1f / GreyGui.Atlas.Width;
        float invH = 1f / GreyGui.Atlas.Height;
        float baseSize = GreyGui.TextSystem.GlyphPixelSize;
        float scale = fontSize / baseSize;

        DrawBatch lastBatch = Batches[^1];
        Rectangle scissor = new(new(0, 0), new(1024, 1024));
        if (
            GreyGui.Atlas != lastBatch.Texture ||
            scissor != lastBatch.Scissor)
        {
            Batches.Add(new DrawBatch
            {
                Texture = GreyGui.Atlas,
                Scissor = scissor,
                IndexOffset = IndexCount,
                IndexCount = 0
            });
            lastBatch = Batches[^1];
        }
        FontInfo fontInfo = GreyGui.TextSystem.GetFontInfo(GreyGui.TextSystem.DefaultFont);
        foreach (char c in text)
        {
            EnsureCapacity(4, 6);
            GlyphInfo info = fontInfo.GlyphInfoMap[c];
            if (info.SrcRect.Width == 0)
            {
                cursor.X += info.AdvanceWidth;
                continue;
            }
            if (c == '\n')
            {
                cursor.X = position.X;
                cursor.Y += fontInfo.TypefaceWrapper.Typeface.LineGap * scale;
                continue;
            }
            float renderX = cursor.X - (info.Offset.X * scale); //- (info.Offset.X * scale)
            float renderY = cursor.Y + (info.Offset.Y * scale);

            Vector2 uv0 = new(info.SrcRect.X * invW, info.SrcRect.Y * invH);
            Vector2 uv1 = new(info.SrcRect.Right * invW, info.SrcRect.Bottom * invH);

            Vector2 renderPos = new(renderX, renderY);
            float w = info.SrcRect.Width * scale;
            float h = info.SrcRect.Height * scale;

            Vector4 textParams = new Vector4(w, h, info.GlyphRange, -1);

            int vOffset = VertexCount;

            SetVertex(VertexCount++, new Vector3(renderPos.X, renderPos.Y, 0), Color.White, Color.White, new Vector2(uv0.X, uv0.Y), new Vector2(0, 0), textParams);
            SetVertex(VertexCount++, new Vector3(renderPos.X + w, renderPos.Y, 0), Color.White, Color.White, new Vector2(uv1.X, uv0.Y), new Vector2(1, 0), textParams);
            SetVertex(VertexCount++, new Vector3(renderPos.X + w, renderPos.Y + h, 0), Color.White, Color.White, new Vector2(uv1.X, uv1.Y), new Vector2(1, 1), textParams);
            SetVertex(VertexCount++, new Vector3(renderPos.X, renderPos.Y + h, 0), Color.White, Color.White, new Vector2(uv0.X, uv1.Y), new Vector2(0, 1), textParams);

            _indices[IndexCount++] = vOffset + 0;
            _indices[IndexCount++] = vOffset + 1;
            _indices[IndexCount++] = vOffset + 2;
            _indices[IndexCount++] = vOffset + 2;
            _indices[IndexCount++] = vOffset + 3;
            _indices[IndexCount++] = vOffset + 0;

            lastBatch.IndexCount += 6;

            cursor.X += fontSize;
        }
        Batches[^1] = lastBatch;

    }

    private void SetVertex(int index, Vector3 pos, Color col, Color borderCol, Vector2 uv, Vector2 local, Vector4 rParams)
    {
        _vertices[index].Position = pos;
        _vertices[index].Color = col;
        _vertices[index].BorderColor = borderCol;
        _vertices[index].TexCoord = uv;
        _vertices[index].LocalCoord = local;
        _vertices[index].RectParams = rParams;
    }

    private void EnsureCapacity(int newVertexCount, int newIndexCount)
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
}