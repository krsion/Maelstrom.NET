using System.Text.Json.Serialization;

namespace MaelstromNode.Models.MessageBodies;

internal class Broadcast : MessageBody
{
    public const string BroadcastType = "broadcast";

    [JsonPropertyName("message")]
    public required int BroadcastMessage { get; set; }
}
