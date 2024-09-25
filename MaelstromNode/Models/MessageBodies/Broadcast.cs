using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

namespace MaelstromNode.Models.MessageBodies;

internal class Broadcast : MessageBody
{
    public const string BroadcastType = "broadcast";

    [JsonConstructor]
    [SetsRequiredMembers]
    public Broadcast(int broadcastMessage) : base()
    {
        Type = BroadcastType;
        BroadcastMessage = broadcastMessage;
    }

    [JsonPropertyName("message")]
    public required int BroadcastMessage { get; set; }
}
