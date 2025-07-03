using Maelstrom.Models;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

namespace CounterService.Models.MessageBodies;

internal class Merge : MessageBody
{
    public const string MergeType = "merge";

    [JsonConstructor]
    [SetsRequiredMembers]
        public Merge(int positiveCounter, int negativeCounter) : base()
    {
        Type = MergeType;
        PositiveCounter = positiveCounter;
        NegativeCounter = negativeCounter;
    }

    [JsonPropertyName("positive_counter")]
    public required int PositiveCounter { get; set; }

    [JsonPropertyName("negative_counter")]
    public required int NegativeCounter { get; set; }
}
