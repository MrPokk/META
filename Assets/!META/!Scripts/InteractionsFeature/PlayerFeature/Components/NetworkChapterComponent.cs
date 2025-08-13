using System;
using Mirror;

[Serializable]
public struct NetworkSyncComponent
{
    public ushort objectId;
    public int ownedId;

    public NetworkSyncComponent(int owned = -1, ushort objectId = 0)
    {
        ownedId = owned;
        this.objectId = objectId;
    }
}
