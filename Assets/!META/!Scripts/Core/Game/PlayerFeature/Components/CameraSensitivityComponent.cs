using UnityEngine;
using Unity.Cinemachine;

public class CameraSensitivityController : MonoBehaviour
{
    [SerializeField] private CinemachineCamera freeLookCamera;
    
    // Сохранение чувствительности в PlayerPrefs
    public float MouseSensitivity
    {
        get => PlayerPrefs.GetFloat("MouseSensitivity", 1f);
        set
        {
            PlayerPrefs.SetFloat("MouseSensitivity", value);
            PlayerPrefs.Save();
            ApplySensitivity();
        }
    }
    
    // Инверсия оси Y
    public bool InvertYAxis
    {
        get => PlayerPrefs.GetInt("InvertY", 0) == 1;
        set
        {
            PlayerPrefs.SetInt("InvertY", value ? 1 : 0);
            PlayerPrefs.Save();
            ApplyInvertY();
        }
    }
    
    void Start()
    {
        if (freeLookCamera == null)
            freeLookCamera = GetComponent<CinemachineCamera>();
        
        ApplySensitivity();
        ApplyInvertY();
    }
    
    private void ApplySensitivity()
    {
        if (freeLookCamera == null) return;
        
    }
    
    private void ApplyInvertY()
    {
        if (freeLookCamera == null) return;
        
    }
    
    // Метод для изменения FOV (поля зрения)
    public void SetFieldOfView(float fov)
    {
        if (freeLookCamera != null)
        {
            PlayerPrefs.SetFloat("CameraFOV", fov);
        }
    }
}
