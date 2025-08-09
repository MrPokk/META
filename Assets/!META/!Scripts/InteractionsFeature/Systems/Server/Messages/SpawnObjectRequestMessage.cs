using Mirror;
using UnityEngine;

public struct SpawnObjectRequestMessage : NetworkMessage
{
    public string prefabId;
    public Vector3 position;
    public Quaternion rotation;

    public SpawnObjectRequestMessage(string prefabId, Vector3 position = default, Quaternion rotation = default) : this()
    {
        this.prefabId = prefabId;
        this.position = position;
        this.rotation = rotation;
    }

}
