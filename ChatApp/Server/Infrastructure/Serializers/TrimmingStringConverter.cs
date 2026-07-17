using System.Text.Json;
using System.Text.Json.Serialization;

namespace Server.Infrastructure.Serializers;

public sealed class TrimmingStringConverter : JsonConverter<string>
{
    public static readonly TrimmingStringConverter Instance = new();

    public override string? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        return reader.GetString()?.Trim();
    }

    public override void Write(Utf8JsonWriter writer, string value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value);
    }
}