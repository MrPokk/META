using System;
using Mirror;
using UnityEngine;

[Serializable]
public struct OwnerSpawnObjectMessage : NetworkMessage
{
    public NetworkSyncComponent connectionId;
    public SerializedType entity;
    public SerializedType view;
    public TransformComponent transformComponent;

    public OwnerSpawnObjectMessage(NetworkSyncComponent id, SerializedType entityType, SerializedType viewType, TransformComponent transform) : this()
    {
        connectionId = id;
        transformComponent = transform;
        entity = entityType;
        view = viewType;
    }
}


