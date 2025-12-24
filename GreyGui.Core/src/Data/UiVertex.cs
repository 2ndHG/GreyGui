using System.Runtime.InteropServices;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GreyGui;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct UiVertex : IVertexType
{
    public Vector3 Position;
    public Color Color;
    public Color BorderColor;
    public Vector2 TexCoord;
    public Vector2 LocalCoord;
    public Vector4 RectParams;  // width, height, border radius, border width

    public static readonly VertexDeclaration VertexDeclaration = new (
        new VertexElement(0, VertexElementFormat.Vector3, VertexElementUsage.Position, 0),
        new VertexElement(12, VertexElementFormat.Color, VertexElementUsage.TextureCoordinate, 3),
        new VertexElement(16, VertexElementFormat.Color, VertexElementUsage.TextureCoordinate, 4),
        new VertexElement(20, VertexElementFormat.Vector2, VertexElementUsage.TextureCoordinate, 0),
        new VertexElement(28, VertexElementFormat.Vector2, VertexElementUsage.TextureCoordinate, 1),
        new VertexElement(36, VertexElementFormat.Vector4, VertexElementUsage.TextureCoordinate, 2)
    );

    readonly VertexDeclaration IVertexType.VertexDeclaration => VertexDeclaration;
}