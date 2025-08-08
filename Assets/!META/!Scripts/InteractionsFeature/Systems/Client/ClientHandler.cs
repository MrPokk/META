using BitterECS.Core;
using Mirror;
using UnityEngine;

public class ClientHandler : IClientConnected
{
    public Priority PrioritySystem => Priority.FIRST_TASK;

    public void Connect()
    {

        NetworkClient.Send(new SceneChangeRequestMessage { sceneType = SceneTypes.StartRoom });
    }
}
