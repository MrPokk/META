using Mirror;

public struct SyncTransformMessage : NetworkMessage
{
    public int entityId;
    public TransformComponent transformComponent;

    public SyncTransformMessage(int id,TransformComponent transform)
    {
        transformComponent = transform;
        entityId = id;
    }
}
