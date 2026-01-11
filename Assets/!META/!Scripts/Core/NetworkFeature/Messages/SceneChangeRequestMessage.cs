using System;
using Mirror;

[Serializable]
public struct SceneChangeRequestMessage : NetworkMessage
{
    public SceneTypes sceneType;

    public SceneChangeRequestMessage(SceneTypes sceneType)
    {
        this.sceneType = sceneType;
    }
}
