#if UNITY_EDITOR
using BitterECS.Core;
using UnityEngine;
using UnityEngine.SceneManagement;

public class DebugUtility : IEcsRunSystem
{
    public Priority PrioritySystem => Priority.High;

    public void Run()
    {
        if (Input.GetKeyDown(KeyCode.R))
            SceneManager.LoadScene(sceneBuildIndex: 0);
    }

}
#endif
