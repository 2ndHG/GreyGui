using System.Text.Json.Serialization;
using Microsoft.Xna.Framework;
namespace GreyGui;

public readonly record struct GlyphInfo
{
    [JsonConverter(typeof(RectangleConverter))]
    [JsonPropertyName("SR")]
    public readonly Rectangle SrcRect { get; init; }
    [JsonPropertyName("AW")]
    public float AdvanceWidth { get; init; }

    [JsonConverter(typeof(Vector2Converter))]
    [JsonPropertyName("O")]
    public Vector2 Origin { get; init; }
    [JsonPropertyName("GR")]
    public float GlyphRange { get; init; }
}