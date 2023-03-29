using System.Dynamic;
using System.Text.Json.Serialization;

namespace Fenrus.Converters;

/// <summary>
/// Converter used when flattening a Language json file to Dictionary
/// </summary>
public class LanguageConverter : JsonConverter<object>
{
    /// <summary>
    /// Write the value as JSON.
    /// </summary>
    /// <remarks>
    /// A converter may throw any Exception, but should throw <cref>JsonException</cref> when the JSON
    /// cannot be created.
    /// </remarks>
    /// <param name="writer">The <see cref="Utf8JsonWriter"/> to write to.</param>
    /// <param name="value">The value to convert.</param>
    /// <param name="options">The <see cref="JsonSerializerOptions"/> being used.</param>
    public override void Write(Utf8JsonWriter writer, object value, JsonSerializerOptions options)
    {
        if (value.GetType() == typeof(object))
        {
            writer.WriteStartObject();
            writer.WriteEndObject();
        }
        else
        {
            JsonSerializer.Serialize(writer, value, value.GetType(), options);
        }
    }

    /// <summary>
    /// Read and convert the JSON to T.
    /// </summary>
    /// <remarks>
    /// A converter may throw any Exception, but should throw <cref>JsonException</cref> when the JSON is invalid.
    /// </remarks>
    /// <param name="reader">The <see cref="Utf8JsonReader"/> to read from.</param>
    /// <param name="typeToConvert">The <see cref="Type"/> being converted.</param>
    /// <param name="options">The <see cref="JsonSerializerOptions"/> being used.</param>
    /// <returns>The value that was converted.</returns>
    public override object Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        switch (reader.TokenType)
        {
            case JsonTokenType.Null:
                return null;
            case JsonTokenType.String:
                return reader.GetString();
            case JsonTokenType.StartArray:
                {
                    var list = new List<object>();
                    while (reader.Read())
                    {
                        switch (reader.TokenType)
                        {
                            default:
                                list.Add(Read(ref reader, typeof(object), options));
                                break;
                            case JsonTokenType.EndArray:
                                return list;
                        }
                    }
                    throw new JsonException();
                }
            case JsonTokenType.StartObject:
                var dict = CreateDictionary();
                while (reader.Read())
                {
                    switch (reader.TokenType)
                    {
                        case JsonTokenType.EndObject:
                            return dict;
                        case JsonTokenType.PropertyName:
                            var key = reader.GetString();
                            reader.Read();
                            dict.Add(key, Read(ref reader, typeof(object), options));
                            break;
                        default:
                            throw new JsonException();
                    }
                }
                throw new JsonException();
            default:
                throw new JsonException(string.Format("Unknown token {0}", reader.TokenType));
        }
    }

    /// <summary>
    /// Creates an IDictionary instance
    /// </summary>
    /// <returns>an IDictionary instance</returns>
    protected virtual IDictionary<string, object> CreateDictionary() => new ExpandoObject() as IDictionary<string, object>;
}