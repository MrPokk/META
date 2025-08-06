using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class SceneLoader
{
    private static SceneConfig s_sceneConfig;

    public static void Initialize(SceneConfig sceneConfig)
    {
        s_sceneConfig = sceneConfig;
    }

    public static async Task LoadSceneAsync(SceneTypes sceneType)
    {
        var sceneName = s_sceneConfig.GetSceneName(sceneType);
        if (string.IsNullOrEmpty(sceneName))
        {
            Debug.LogError($"Scene name for {sceneType} is not set!");
            return;
        }

        var asyncOp = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Single);
        if (asyncOp == null)
        {
#if UNITY_EDITOR
            Debug.LogError($"Failed to load scene: {sceneName}"); return;
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
