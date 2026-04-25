using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Xna.Framework;

namespace GreyGui;

public class RectangleConverter : JsonConverter<Rectangle>
{
    public override Rectangle Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        using JsonDocument document = JsonDocument.ParseValue(ref reader);
        var root = document.RootElement;

        int X = root.GetProperty("X").GetInt32();
        int Y = root.GetProperty("Y").GetInt32();
        int W = root.GetProperty("W").GetInt32();
        int H = root.GetProperty("H").GetInt32();
        return new Rectangle(X, Y, W, H);
    }

    public override void Write(Utf8JsonWriter writer, Rectangle value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();
        writer.WriteNumber("X", value.X);
        writer.WriteNumber("Y", value.Y);
        writer.WriteNumber("W", value.Width);
        writer.WriteNumber("H", value.Height);
        writer.WriteEndObject();
    }
}