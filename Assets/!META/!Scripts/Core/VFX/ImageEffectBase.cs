using UnityEngine;

[RequireComponent(typeof(Camera))]
public class ImageEffectBase : MonoBehaviour
{
    [SerializeField] protected Shader _shader;
    protected Material _material;

    protected virtual void Awake()
    {
        _material = new Material(_shader);
    }

    protected virtual void OnRenderImage(RenderTexture src, RenderTexture dst)
    {
        Graphics.Blit(src, dst, _material);
    }
}
