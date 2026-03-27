using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace R10.Web.Helpers
{
    /// <summary>
    /// Handles empty string values for DateTime? during JSON deserialization.
    /// The formDataToJson JS function may send empty strings for date fields.
    /// </summary>
    public class NullableDateTimeConverter : JsonConverter<DateTime?>
    {
        public override DateTime? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.String)
            {
                var str = reader.GetString();
                if (string.IsNullOrWhiteSpace(str))
                    return null;
                if (DateTime.TryParse(str, out var dt))
                    return dt;
                return null;
            }
            if (reader.TokenType == JsonTokenType.Null)
                return null;
            return reader.GetDateTime();
        }

        public override void Write(Utf8JsonWriter writer, DateTime? value, JsonSerializerOptions options)
        {
            if (value.HasValue)
                writer.WriteStringValue(value.Value);
            else
                writer.WriteNullValue();
        }
    }
}
