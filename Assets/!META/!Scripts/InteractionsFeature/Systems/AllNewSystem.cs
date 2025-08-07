
using BitterECS.Core;
using Mirror;

public interface IClientConnected : IEcsSystem
{
    public void Connect();
}

public interface IClientDisconnected : IEcsSystem
{
    public void Disconnect();
}

public interface IClientError : IEcsSystem
{
    public void OnError();
}

public interface IClientConnectedRun : IEcsSystem
{
    public void Run();
}

internal interface IClientConnectedFixedRun : IEcsSystem
{
    void FixedRun();
}
public interface IServerStart : IEcsSystem
{
    public void Start();
}

public interface IServerConnected : IEcsSystem
{
    public void Connect(NetworkConnectionToClient client);
}

public interface IServerDisconnected : IEcsSystem
{
    public void Disconnect(NetworkConnectionToClient client);
}

public interface IServerError : IEcsSystem
{
    public void OnError(NetworkConnectionToClient client, TransportError error, string arg3);
}

public interface IServerConnectedRun : IEcsSystem
{
    public void Run();
}

internal interface IServerConnectedFixedRun : IEcsSystem
{
    void FixedRun();
}
