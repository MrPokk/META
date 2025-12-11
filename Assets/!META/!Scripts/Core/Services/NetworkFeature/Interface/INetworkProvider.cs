using BitterECS.Core;

public interface IProviderHandler
{
    public void HandlersClient();
    public void HandlersServer();
}


public interface IPlayerAdd : IEcsSystem, IEcsAutoImplement 
{
    public void AddPlayer();
}
