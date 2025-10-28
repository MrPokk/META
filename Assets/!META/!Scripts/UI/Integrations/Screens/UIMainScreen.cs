using UnityEngine;
using UnityEngine.UI;
using VContainer;

public class UIMainScreen : UIScreen
{
    [SerializeField] private Button _btnGoToGameplay;

    private void OnEnable()
    {
        _btnGoToGameplay.onClick.AddListener(OnGoToGameplayButtonClicked);
    }

    private void OnDisable()
    {
        _btnGoToGameplay.onClick.RemoveListener(OnGoToGameplayButtonClicked);
    }

    private void OnGoToGameplayButtonClicked()
    {
        Container.Resolve<EntryPointClient>().SetupConnection();
        SceneNetworkProvider.ChangeScene(SceneTypes.StartRoom);
        Close();
    }
}
