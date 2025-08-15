public class UIMainScene : UIScreen
{
    public void ConnectGame()
    {
        SceneNetworkProvider.ChangeScene(SceneTypes.StartRoom);
    }
}
