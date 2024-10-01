using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

namespace MaelstromNode.Models.MessageBodies;

internal class CommitOffsetsOk : MessageBody
{
    public const string CommitOffsetsOkType = "commit_offsets_ok";

    [JsonConstructor]
    [SetsRequiredMembers]
    public CommitOffsetsOk() : base()
    {
        Type = CommitOffsetsOkType;
    }
}
