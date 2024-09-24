using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

namespace MaelstromNode.Models.MessageBodies;

internal class TopologyOk : MessageBody
{
    public const string TopologyOkType = "topology_ok";

    [JsonConstructor]
    [SetsRequiredMembers]
    public TopologyOk() : base()
    {
        Type = TopologyOkType;
    }
}
