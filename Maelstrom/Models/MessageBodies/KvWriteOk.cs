using Maelstrom.Models;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

namespace Maelstrom.Models.MessageBodies;

internal class KvWriteOk : MessageBody
{
    public const string WriteOkType = "write_ok";

    [JsonConstructor]
    [SetsRequiredMembers]
    public KvWriteOk() : base()
    {
        Type = WriteOkType;
    }
}