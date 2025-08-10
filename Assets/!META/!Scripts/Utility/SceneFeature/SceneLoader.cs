using System.Threading.Tasks;
using UnityEngine;
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
        var sceneName = s_sceneConfig.GetSceneName(sceneType);
        if (string.IsNullOrEmpty(sceneName))
        {
            LoggerUtility.Error($"Scene name for {sceneType} is not set!");
            return;
        }

        SceneManager.LoadScene(sceneName, LoadSceneMode.Single);
    }

    public static void LoadScene(SceneTypes sceneType, LoadSceneParameters loadSceneParameters)
    {
        var sceneName = s_sceneConfig.GetSceneName(sceneType);
        if (string.IsNullOrEmpty(sceneName))
        {
            LoggerUtility.Error($"Scene name for {sceneType} is not set!");
            return;
        }

        SceneManager.LoadScene(sceneName, loadSceneParameters);
    }

    public static async Task LoadSceneAsync(SceneTypes sceneType)
    {
        var sceneName = s_sceneConfig.GetSceneName(sceneType);
        if (string.IsNullOrEmpty(sceneName))
        {
            LoggerUtility.Error($"Scene name for {sceneType} is not set!");
            return;
        }

        var asyncOp = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Single);
        if (asyncOp == null)
        {
#if UNITY_EDITOR
            LoggerUtility.Error($"Failed to load scene: {sceneName}"); return;
#else
            return;
#endif
        }
        asyncOp.allowSceneActivation = true;

        while (!asyncOp.isDone)
        {
            await Task.Yield();
        }
    }

    public static async Task LoadSceneAsync(SceneTypes sceneType, LoadSceneParameters loadSceneParameters)
    {
        var sceneName = s_sceneConfig.GetSceneName(sceneType);
        if (string.IsNullOrEmpty(sceneName))
        {
            LoggerUtility.Error($"Scene name for {sceneType} is not set!");
            return;
        }

        var asyncOp = SceneManager.LoadSceneAsync(sceneName, loadSceneParameters);
        if (asyncOp == null)
        {
#if UNITY_EDITOR
            LoggerUtility.Error($"Failed to load scene: {sceneName}"); return;
#else
            return;
#endif
        }
        asyncOp.allowSceneActivation = true;

        while (!asyncOp.isDone)
        {
            await Task.Yield();
        }
    }
}
