using System;
using Mirror;
using UnityEngine;

[Serializable]
public struct SyncObjectSpawn : NetworkMessage
{
    public uint assetId;
    public SerializedType entity;
    public SerializedType view;
    public Vector3 position;
    public Quaternion rotation;

    public SyncObjectSpawn(SerializedType entityType, SerializedType viewType, Vector3 position, Quaternion rotation) : this()
    {
        entity = entityType;
        view = viewType;
        this.position = position;
        this.rotation = rotation;
    }

    public SyncObjectSpawn(SyncObjectSpawn spawn, uint assetId) : this()
    {
        entity = spawn.entity;
        view = spawn.view;
        position = spawn.position;
        rotation = spawn.rotation;
        this.assetId = assetId;
    }
}


