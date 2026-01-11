using Mirror;

public struct ChatMessage : NetworkMessage
{
    public uint ownerId;
    public string message;
    public string sender;

    public ChatMessage(uint ownerId, string message = default, string sender = default)
    {
        this.message = message;
        this.sender = sender;
        this.ownerId = ownerId;
    }
}
