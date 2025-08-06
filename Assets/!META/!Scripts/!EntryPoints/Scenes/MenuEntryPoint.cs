using Mirror;
using UnityEngine;
using VContainer;
using VContainer.Unity;

public class MenuEntryPoint : LifetimeScope
{
    private NetworkManager _networkManager;

    private void Start()
    {
        _networkManager = Parent.Container.Resolve<NetworkManager>();
    }
    
    public void ReConnect()
    {
        _networkManager.StartClient();
    }
}
