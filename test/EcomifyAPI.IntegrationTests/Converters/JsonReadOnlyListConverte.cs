using System.Text.Json;
using System.Text.Json.Serialization;

namespace EcomifyAPI.IntegrationTests.Converters;

public class JsonReadOnlyListConverter<T> : JsonConverter<IReadOnlyList<T>>
{
    public override IReadOnlyList<T> Read(
        ref Utf8JsonReader reader,
        Type typeToConvert,
        JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.StartArray)
            throw new JsonException();

        var list = new List<T>();

        while (reader.Read())
        {
            if (reader.TokenType == JsonTokenType.EndArray)
                return list;

            var value = JsonSerializer.Deserialize<T>(ref reader, options);
            if (value is not null)
            {
                list.Add(value);
            }
        }

        throw new JsonException();
    }

    public override void Write(
        Utf8JsonWriter writer,
        IReadOnlyList<T> value,
        JsonSerializerOptions options)
    {
        writer.WriteStartArray();
        foreach (var item in value)
        {
            JsonSerializer.Serialize(writer, item, options);
        }
        writer.WriteEndArray();
    }
}