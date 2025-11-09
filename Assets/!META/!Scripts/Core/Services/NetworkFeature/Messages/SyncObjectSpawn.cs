using System;
using Mirror;
using UnityEngine;

[Serializable]
public struct SyncObjectSpawn : NetworkMessage
{
    public uint netId;

    public SyncObjectSpawn(uint netId) : this()
    {
        this.netId = netId;
    }

    public SerializedType entity;
    public Vector3 position;
    public Quaternion rotation;

    public SyncObjectSpawn(SerializedType entityType, Vector3 position, Quaternion rotation) : this()
    {
        entity = entityType;
        this.position = position;
        this.rotation = rotation;
    }

    public SyncObjectSpawn(SyncObjectSpawn spawn, uint netId) : this()
    {
        entity = spawn.entity;
        position = spawn.position;
        rotation = spawn.rotation;
        this.netId = netId;
    }
}


