using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

namespace MaelstromNode.Models.MessageBodies;

internal class KvCas<T, U> : MessageBody
{
    public const string CasType = "cas";

    [JsonConstructor]
    [SetsRequiredMembers]
    public KvCas(T key, U from, U to) : base()
    {
        Type = CasType;
        Key = key;
        From = from;
        To = to;
    }

    [JsonPropertyName("key")]
    public required T Key { get; set; }

    [JsonPropertyName("from")]
    public required U From { get; set; }

    [JsonPropertyName("to")]
    public required U To { get; set; }
}
