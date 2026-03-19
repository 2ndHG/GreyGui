namespace GreyGui;

public struct TextSegment
{
    public float WidthWithSpace => width + pendingSpaceWidth;
    public int CharCount => nonSpaceCount + pendingSpaceCount;
    public int startIndex;
    public ushort nonSpaceCount;
    public ushort pendingSpaceCount;
    public float width;
    public float pendingSpaceWidth;
}