using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine.SceneManagement;

public class SceneLoader
{
    private static SceneConfig s_sceneConfig;
    private static HashSet<Scene> s_scenesToLoadServer;

    public void Initialize(SceneConfig sceneConfig)
    {
        s_sceneConfig = sceneConfig;
        s_scenesToLoadServer = new(s_sceneConfig.GetServerLoadScenes().Count);
    }

    public static void LoadScene(SceneTypes sceneType)
    {
        var sceneName = SceneConfig.GetSceneName(sceneType);
        SceneManager.LoadScene(sceneName, LoadSceneMode.Single);
    }

    public static Scene LoadScene(SceneTypes sceneType, LoadSceneParameters loadSceneParameters)
    {
        var sceneName = SceneConfig.GetSceneName(sceneType);
        return SceneManager.LoadScene(sceneName, loadSceneParameters);
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

    public static void AddServerScene(SceneTypes types, Scene sceneToServer)
    {
        if (SceneConfig.IsServerScene(types))
        {
            s_scenesToLoadServer.Add(sceneToServer);
        }
        else
        {
            LoggerUtility.Error($"Scene {types} is not a server scene!");
        }
    }
}
