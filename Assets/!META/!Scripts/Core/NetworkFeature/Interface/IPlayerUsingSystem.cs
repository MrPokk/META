using BitterECS.Core;

public interface IPlayerUsingSystem : IEcsAutoImplement
{
    public void OnRun(PlayerProvider player);
}
