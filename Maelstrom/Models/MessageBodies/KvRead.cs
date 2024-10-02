using Maelstrom.Models;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

namespace Maelstrom.Models.MessageBodies;

internal class KvRead<T> : MessageBody
{
    public const string ReadType = "read";

    [JsonConstructor]
    [SetsRequiredMembers]
    public KvRead(T key) : base()
    {
        Type = ReadType;
        Key = key;
    }

    [JsonPropertyName("key")]
    public required T Key { get; set; }
}
