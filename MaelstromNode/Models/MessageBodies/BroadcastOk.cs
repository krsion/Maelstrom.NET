using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

namespace MaelstromNode.Models.MessageBodies;

internal class BroadcastOk : MessageBody
{
    public const string BroadcastOkType = "broadcast_ok";

    [JsonConstructor]
    [SetsRequiredMembers]
    public BroadcastOk() : base()
    {
        Type = BroadcastOkType;
    }
}
