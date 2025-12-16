// CelShadingRendererFeature.cs
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.Rendering.RenderGraphModule;

public sealed class CelShadingRendererFeature : ScriptableRendererFeature
{
    [System.Serializable]
    public class Settings
    {
        public float threshold = 0.5f;
        public int bands = 3;
        public RenderPassEvent passEvent = RenderPassEvent.AfterRenderingPostProcessing;
    }

    public Settings settings = new Settings();
    private CelShadingRenderPass _pass;

    public override void Create()
    {
        _pass = new CelShadingRenderPass(settings);
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        // Добавляем пасс только для игровой камеры.
        if (renderingData.cameraData.cameraType == CameraType.Game)
        {
            renderer.EnqueuePass(_pass);
        }
    }

    protected override void Dispose(bool disposing)
    {
        _pass?.Dispose();
        base.Dispose(disposing);
    }
}
