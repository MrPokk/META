using UnityEngine;
using VContainer;

public class UIReconnectScreen : UIScreen
{
    [SerializeField] private UIButtonProvider _btnGoToReconnect;
    [SerializeField] private UIButtonProvider _btnGoToExit;


    public override void Open()
    {
        AddListener();

        UINavigationComponent
            .UsingNavigation(gameObject)
            .ApplyNavigation(
                _btnGoToReconnect,
                _btnGoToExit);

        base.Open();
    }

    public override void Close()
    {
        RemoveListener();
        base.Close();
    }

    private void AddListener()
    {
        _btnGoToReconnect.AddListener(OnGoToGameplayButtOnClicked);
        _btnGoToExit.AddListener(OnGoToExitButtOnClicked);
    }

    private void RemoveListener()
    {
        _btnGoToReconnect.RemoveListener(OnGoToGameplayButtOnClicked);
        _btnGoToExit.RemoveListener(OnGoToExitButtOnClicked);
    }

    private void OnGoToExitButtOnClicked()
    {
        Application.Quit();
    }

    private void OnGoToGameplayButtOnClicked()
    {
        Container.Resolve<EntryPointClient>().SetupConnection();
        SceneNetworkProvider.ChangeScene(SceneTypes.StartFloor);

        if (NetworkUtility.IsClientActive())
        {
            Close();
        }
    }
}
