using System;
using Mirror;
using UnityEngine;

[Serializable]
public struct SpawnObjectMessage : NetworkMessage
{
    public NetworkSyncComponent connectionId;
    public string entityTypeName;
    public string viewTypeName;
    public TransformComponent transformComponent;
    public SpawnObjectMessage(int id, Type entityType, Type viewType, TransformComponent transform) : this()
    {
        connectionId = new NetworkSyncComponent(id);
        transformComponent = transform;
        entityTypeName = entityType.AssemblyQualifiedName;
        viewTypeName = viewType.AssemblyQualifiedName;
    }
}


