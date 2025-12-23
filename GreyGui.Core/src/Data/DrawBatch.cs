using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GreyGui;

public struct DrawBatch
{
    public Texture2D Texture;
    public Rectangle Scissor;
    public int IndexOffset;
    public int IndexCount;
}