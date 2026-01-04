namespace GreyGui;
public interface IRatioElement
{
    public bool UseWidthRatio { get; }
    public bool UseHeightRatio { get; }
    public bool UseHeightWidthRatio { get; }
    public float WidthRatio { get; }
    public float HeightWidthRatio { get; }
    public float HeightRatio { get; }
}