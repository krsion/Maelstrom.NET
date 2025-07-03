using Maelstrom.Models;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

namespace CounterService.Models.MessageBodies;

internal class MergeOk : MessageBody
{
    public const string MergeOkType = "merge_ok";

    [JsonConstructor]
    [SetsRequiredMembers]
    public MergeOk(int positiveCounter, int negativeCounter) : base()
    {
        Type = MergeOkType;
        PositiveCounter = positiveCounter;
        NegativeCounter = negativeCounter;
    }

    [JsonPropertyName("positive_counter")]
    public required int PositiveCounter { get; set; }

    [JsonPropertyName("negative_counter")]
    public required int NegativeCounter { get; set; }
}