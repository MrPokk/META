using UnityEngine;

public class DontDestroyComponent : MonoBehaviour
{
    private void Start()
    {
        DontDestroyOnLoad(gameObject);
    }
}
