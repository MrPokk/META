using UnityEngine;

public class UIContainer : MonoBehaviour
{
    public void Disable() => gameObject.SetActive(false);
    public void Enable() => gameObject.SetActive(true);
    public void Toggle() => gameObject.SetActive(!gameObject.activeSelf);
}
