namespace GreyGui;
public interface IPercentElement
{
    public bool UsePercentWidth { get; }
    public bool UseHeightWidthRatio { get; }
    public float WidthPercent { get; }
    public float HeightWidthRatio { get; }
}