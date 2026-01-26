
using System.Text.Json;
using System.Text.Json.Serialization;

namespace DoraExplorer.DotNetTool;

public class DateTimeConverterUsingDateTimeParse : JsonConverter<DateTime>
{
    public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        return DateTime.Parse(reader.GetString() ?? string.Empty);
    }

    public override bool CanConvert(Type typeToConvert) => typeToConvert == typeof(DateTime);
    public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options) => throw new NotImplementedException();
}
public class DateTimeOffsetConverterUsingDateTimeParse : JsonConverter<DateTimeOffset>
{
    public override DateTimeOffset Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        return DateTimeOffset.Parse(reader.GetString() ?? string.Empty);
    }

    public override bool CanConvert(Type typeToConvert) => typeToConvert == typeof(DateTimeOffset);
    public override void Write(Utf8JsonWriter writer, DateTimeOffset value, JsonSerializerOptions options) => throw new NotImplementedException();
}


