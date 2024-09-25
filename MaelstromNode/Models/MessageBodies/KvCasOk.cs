using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

namespace MaelstromNode.Models.MessageBodies;

internal class KvCasOk : MessageBody
{
    public const string CasOkType = "cas_ok";

    [JsonConstructor]
    [SetsRequiredMembers]
    public KvCasOk() : base()
    {
        Type = CasOkType;
    }
}