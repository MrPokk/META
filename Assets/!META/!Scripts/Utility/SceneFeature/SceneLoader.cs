using Cysharp.Threading.Tasks;
using UnityEngine.SceneManagement;

public class SceneLoader
{
    private static SceneConfig s_sceneConfig;

    public static SceneLoader Initialize(SceneConfig sceneConfig)
    {
        s_sceneConfig = sceneConfig;
        return new SceneLoader();
    }

    public static void LoadScene(SceneTypes sceneType)
    {
        var sceneName = SceneConfig.GetSceneName(sceneType);
        SceneManager.LoadScene(sceneName, LoadSceneMode.Single);
    }

    public static void LoadScene(SceneTypes sceneType, LoadSceneParameters loadSceneParameters)
    {
        var sceneName = SceneConfig.GetSceneName(sceneType);
        SceneManager.LoadScene(sceneName, loadSceneParameters);
    }

    public static async UniTask LoadSceneAsync(SceneTypes sceneType)
    {
        var sceneName = SceneConfig.GetSceneName(sceneType);
        var asyncOp = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Single);
        asyncOp.allowSceneActivation = true;
        await asyncOp.ToUniTask();
    }

    public static async UniTask LoadSceneAsync(SceneTypes sceneType, LoadSceneParameters loadSceneParameters)
    {
        var sceneName = SceneConfig.GetSceneName(sceneType);
        var asyncOp = SceneManager.LoadSceneAsync(sceneName, loadSceneParameters);
        asyncOp.allowSceneActivation = true;
        await asyncOp.ToUniTask();
    }

    public static async UniTask LoadSceneAsync(SceneTypes sceneType, System.Action onComplete = null)
    {
        var sceneName = SceneConfig.GetSceneName(sceneType);
        var asyncOp = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Single);
        asyncOp.allowSceneActivation = true;
        await asyncOp.ToUniTask();
        onComplete?.Invoke();
    }

    public static async UniTask LoadSceneAsync(SceneTypes sceneType, LoadSceneParameters loadSceneParameters, System.Action onComplete = null)
    {
        var sceneName = SceneConfig.GetSceneName(sceneType);
        var asyncOp = SceneManager.LoadSceneAsync(sceneName, loadSceneParameters);
        asyncOp.allowSceneActivation = true;
        await asyncOp.ToUniTask();
        onComplete?.Invoke();
    }
}
