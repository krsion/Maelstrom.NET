using System.Text.Json.Serialization;

namespace MaelstromNode.Models.MessageBodies;

internal class ListCommittedOffsets : MessageBody
{
    public const string ListCommittedOffsetsType = "list_committed_offsets";

    [JsonPropertyName("keys")]
    public required List<string> Keys { get; set; }
}
