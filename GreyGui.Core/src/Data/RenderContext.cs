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