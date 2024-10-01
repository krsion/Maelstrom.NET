using System.Text.Json.Serialization;

namespace MaelstromNode.Models.MessageBodies;

internal class Poll : MessageBody
{
    public const string PollType = "poll";

    [JsonPropertyName("offsets")]
    public required Dictionary<string, int> Offsets { get; set; }
}
