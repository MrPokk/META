
using BitterECS.Core;
using Mirror;

#region Client
public interface IClientStart : IEcsAutoImplement
{
    public void Start();
}

public interface IClientConnected : IEcsAutoImplement
{
    public void Connect();
}

public interface IClientSpawn : IEcsAutoImplement
{
    public void Spawn();
}

public interface IClientSceneTransitionStart : IEcsAutoImplement
{
    public void OnStart();
}

public interface IClientSceneTransitionComplete : IEcsAutoImplement
{
    public void OnComplete();
}

public interface IClientDisconnected : IEcsAutoImplement
{
    public void Disconnect();
}

public interface IClientError : IEcsAutoImplement
{
    public void OnError();
}

public interface IClientConnectedRun : IEcsAutoImplement
{
    public void Run();
}

internal interface IClientConnectedFixedRun : IEcsAutoImplement
{
    void FixedRun();
}
#endregion



#region Server
public interface IServerStart : IEcsAutoImplement
{
    public void Start();
}

public interface IServerConnected : IEcsAutoImplement
{
    public void Connect(NetworkConnectionToClient client);
}

public interface IServerDisconnected : IEcsAutoImplement
{
    public void Disconnect(NetworkConnectionToClient client);
}

public interface IServerError : IEcsAutoImplement
{
    public void OnError(NetworkConnectionToClient client, TransportError error, string arg3);
}

public interface IServerConnectedRun : IEcsAutoImplement
{
    public void Run();
}

internal interface IServerConnectedFixedRun : IEcsAutoImplement
{
    void FixedRun();
}
#endregion
