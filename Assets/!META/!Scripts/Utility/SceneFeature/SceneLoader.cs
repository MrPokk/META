using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
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
    public static void LoadScene(SceneTypes sceneType, Action onComplete = null, Action onStart = null)
    {
        LoadSceneInternal(sceneType, LoadSceneMode.Single, onComplete, onStart);
    }

    public static Scene LoadScene(SceneTypes sceneType, LoadSceneParameters loadSceneParameters, Action onStart = null)
    {
        onStart?.Invoke();
        var sceneName = SceneConfig.GetSceneName(sceneType);
        return SceneManager.LoadScene(sceneName, loadSceneParameters);
    }

    public static async UniTask LoadSceneAsync(SceneTypes sceneType, LoadSceneParameters loadSceneParameters = default, Action onStart = null, Action onComplete = null)
    {
        await LoadSceneAsyncInternal(sceneType, LoadSceneMode.Single, loadSceneParameters, onComplete, onStart);
    }

    private static void LoadSceneInternal(SceneTypes sceneType, LoadSceneMode loadSceneMode, Action onComplete = null, Action onStart = null)
    {
        onStart?.Invoke();
        var sceneName = SceneConfig.GetSceneName(sceneType);
        SceneManager.LoadScene(sceneName, loadSceneMode);
        onComplete?.Invoke();
    }

    private static async UniTask LoadSceneAsyncInternal(
        SceneTypes sceneType,
        LoadSceneMode loadSceneMode,
        LoadSceneParameters loadSceneParameters = default,
        Action onComplete = null,
        Action onStart = null)
    {
        onStart?.Invoke();
        var sceneName = SceneConfig.GetSceneName(sceneType);

        var asyncOp = loadSceneParameters.Equals(default(LoadSceneParameters))
            ? SceneManager.LoadSceneAsync(sceneName, loadSceneParameters)
            : SceneManager.LoadSceneAsync(sceneName, loadSceneMode);

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
