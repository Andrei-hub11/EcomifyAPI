using System.Text.Json;
using System.Text.Json.Serialization;

namespace EcomifyAPI.IntegrationTests.Converters;

public class JsonStringSetConverter : JsonConverter<IReadOnlySet<string>>
{
    public override IReadOnlySet<string> Read(
        ref Utf8JsonReader reader,
        Type typeToConvert,
        JsonSerializerOptions options
    )
    {
        if (reader.TokenType != JsonTokenType.StartArray)
        {
            throw new JsonException();
        }

        var set = new HashSet<string>();

        while (reader.Read())
        {
            if (reader.TokenType == JsonTokenType.EndArray)
            {
                return set;
            }

            if (reader.TokenType == JsonTokenType.String)
            {
                set.Add(reader.GetString()!);
            }
        }

        throw new JsonException();
    }

    public override void Write(
        Utf8JsonWriter writer,
        IReadOnlySet<string> value,
        JsonSerializerOptions options
    )
    {
        writer.WriteStartArray();
        foreach (var item in value)
        {
            writer.WriteStringValue(item);
        }
        writer.WriteEndArray();
    }
}