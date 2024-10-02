using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace Maelstrom.Models;

public class Message
{
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
        DeserializeAs<MessageBody>();
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

    public void DeserializeAs<T>() where T : MessageBody
    {
        Body = RawBody.Deserialize<T>();
    }

    private static readonly JsonSerializerOptions _jsonSerializerOptions = new()
    {
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    };

    public string Serialize()
    {
        // A bit of a hack until I can figure out a better way to do this.
        RawBody = JsonSerializer.Deserialize<JsonObject>(Body.Serialize());
        return JsonSerializer.Serialize(this, _jsonSerializerOptions);
    }
}
