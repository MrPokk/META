
using BitterECS.Core;

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

public interface IServerConnected : IEcsSystem
{
    public void Connect();
}

public interface IServerDisconnected : IEcsSystem
{
    public void Disconnect();
}

public interface IServerError : IEcsSystem
{
    public void OnError();
}

public interface IServerConnectedRun : IEcsSystem
{
    public void Run();
}

internal interface IServerConnectedFixedRun : IEcsSystem
{
    void FixedRun();
}
