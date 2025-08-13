using Mirror;

public struct SyncTransformMessage : NetworkMessage
{
    public SerializedType entity;
    public NetworkSyncComponent connectionId;
    public TransformComponent transformComponent;

    public SyncTransformMessage(NetworkSyncComponent id, TransformComponent transform, SerializedType entity)
    {
        transformComponent = transform;
        connectionId = id;
        this.entity = entity;
    }
}
