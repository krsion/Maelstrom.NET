using System.Text.Json.Serialization;

namespace MaelstromNode.Models.MessageBodies;

public class Init : MessageBody
{
    public const string InitType = "init";

    [JsonPropertyName("node_id")]
    public required string NodeId { get; set; }

    [JsonPropertyName("node_ids")]
    public required string[] NodeIds { get; set; }
}
