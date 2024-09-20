using System.Text.Json.Serialization;

namespace MaelstromNode.Models.MessageBodies;

internal class Echo : MessageBody
{
    public const string EchoType = "echo";

    [JsonPropertyName("echo")]
    public required string EchoMessage { get; set; }
}
