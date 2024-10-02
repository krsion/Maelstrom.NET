using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

namespace Maelstrom.Models.MessageBodies;

internal class KvReadOk<T> : MessageBody
{
    public const string ReadOkType = "read_ok";

    [JsonConstructor]
    [SetsRequiredMembers]
    public KvReadOk(T value) : base()
    {
        Type = ReadOkType;
        Value = value;
    }

    [JsonPropertyName("value")]
    public required T Value { get; set; }
}