namespace MaelstromNode;

[AttributeUsage(AttributeTargets.Method)]
internal class MaelstromHandlerAttribute(string messageType) : Attribute
{
    public string MessageType { get; } = messageType;
}
