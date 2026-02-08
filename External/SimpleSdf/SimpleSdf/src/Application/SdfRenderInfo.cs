using System.Numerics;

namespace SimpleSdf;

public record struct SdfRenderInfo
{
    public float FontSize { get; set; }
    public float Range { get; set; }
    public int BitmapWidth { get; set; }
    public int BitmapHeight { get; set; }
    public Vector2 Origin { get; set; }
    public float AdvanceWidth { get; set; }

}