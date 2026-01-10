using Mirror;

public struct ChatMessage : NetworkMessage
{
    public string message;
    public string sender;

    public ChatMessage(string message = default, string sender = default)
    {
        this.message = message;
        this.sender = sender;
    }
}
