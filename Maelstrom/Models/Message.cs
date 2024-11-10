using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace Maelstrom.Models;

public class Message
{
    [SetsRequiredMembers]
    public Message(string src, string dest, MessageBody body)
    {
        Src = src;
        Dest = dest;
        Body = body;
    }

    [JsonConstructor]
    public Message(string src, string dest, JsonObject rawBody)
    {
        Src = src;
        Dest = dest;
        RawBody = rawBody;
        Body = DeserializeAs<MessageBody>();
    }

    [JsonPropertyName("src")]
    [JsonRequired]
    public string Src { get; set; }

    [JsonPropertyName("dest")]
    [JsonRequired]
    public string Dest { get; set; }

    [JsonPropertyName("body")]
    [JsonRequired]
    public JsonObject? RawBody { get; set; }

    [JsonIgnore]
    public MessageBody Body { get; set; }

    public T DeserializeAs<T>() where T : MessageBody
    {
        return RawBody.Deserialize<T>() ?? throw new Exception($"Failed to deserialize message body as {typeof(T)}/");
    }

    private static readonly JsonSerializerOptions _jsonSerializerOptions = new()
    {
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    };

    public string Serialize()
    {
        // A bit of a hack until I can figure out a better way to do this.
        RawBody = JsonSerializer.Deserialize<JsonObject>(Body.Serialize() ?? throw new Exception("Failed to serialize body"));
        return JsonSerializer.Serialize(this, _jsonSerializerOptions);
    }
}
