using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

namespace MaelstromNode.Models.MessageBodies;

internal class KvWrite<T, U> : MessageBody
{
    public const string WriteType = "write";

    [JsonConstructor]
    [SetsRequiredMembers]
    public KvWrite(T key, U value) : base()
    {
        Type = WriteType;
        Key = key;
        Value = value;
    }

    [JsonPropertyName("key")]
    public required T Key { get; set; }

    [JsonPropertyName("value")]
    public required U Value { get; set; }
}
