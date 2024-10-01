using System.Text.Json.Serialization;

namespace MaelstromNode.Models.MessageBodies;

internal class CommitOffsets : MessageBody
{
    public const string CommitOffsetsType = "commit_offsets";

    [JsonPropertyName("offsets")]
    public required Dictionary<string, int> Offsets { get; set; }
}
