using System;
using Mirror;
using UnityEngine;

public struct SpawnObjectMessage : NetworkMessage
{
    public Guid viewId;
    public Guid entityId;
    public Vector3 position;
    public Quaternion rotation;

    public SpawnObjectMessage(Vector3 position = default, Quaternion rotation = default) : this()
    {
        this.position = position;
        this.rotation = rotation;
        viewId = Guid.Empty;
        entityId = Guid.Empty;
    }
}
