// CelShadingRenderPass.cs
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.Rendering.RenderGraphModule;

public sealed class CelShadingRenderPass : ScriptableRenderPass
{
    private readonly CelShadingRendererFeature.Settings _settings;
    private Material _material;
    private static readonly int ThresholdID = Shader.PropertyToID("_Threshold");
    private static readonly int BandsID = Shader.PropertyToID("_Bands");

    public CelShadingRenderPass(CelShadingRendererFeature.Settings settings)
    {
        _settings = settings;
        renderPassEvent = settings.passEvent;

        // Загружаем шейдер и создаём материал.
        Shader shader = Shader.Find("PostProcessing/CelShading");
        if (shader != null)
            _material = new Material(shader);
        else
            Debug.LogError("CelShading shader not found.");
    }

    // Метод Dispose для освобождения материала.
    public void Dispose()
    {
        CoreUtils.Destroy(_material);
    }

    // В URP 17 основной код пасса пишется здесь, а не в Execute.
    public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
    {
        if (_material == null)
            return;

        // 1. Получаем ресурсы камеры.
        UniversalResourceData resourceData = frameData.Get<UniversalResourceData>();
        UniversalCameraData cameraData = frameData.Get<UniversalCameraData>();

        // 2. Создаём текстуру-приёмник для результата.
        RenderTextureDescriptor desc = cameraData.cameraTargetDescriptor;
        desc.msaaSamples = 1;
        desc.depthBufferBits = 0;
        TextureHandle destination = UniversalRenderer.CreateRenderGraphTexture(
            renderGraph, desc, "CelShadingDestination", false);

        // 3. Настраиваем параметры материала.
        _material.SetFloat(ThresholdID, _settings.threshold);
        _material.SetInt(BandsID, _settings.bands);

        // 4. Добавляем рендер-пасс в граф.
        using (var builder = renderGraph.AddRasterRenderPass<PassData>("CelShading Pass", out var passData))
        {
            // Указываем, что пасс читает текстуру цвета камеры.
            passData.sourceTexture = resourceData.activeColorTexture;
            builder.UseTexture(passData.sourceTexture);

            // Указываем текстуру назначения как цель рендеринга.
            builder.SetRenderAttachment(destination, 0);

            // Передаём материал в функцию выполнения.
            passData.material = _material;
            builder.SetRenderFunc<PassData>(
                (PassData data, RasterGraphContext context) => ExecutePass(data, context));
        }

        // 5. Результат пасса записывается в текстуру назначения.
        // Её можно использовать в следующих пассах или как итоговый цвет.
    }

    // Данные, передаваемые в функцию выполнения.
    private class PassData
    {
        public TextureHandle sourceTexture;
        public Material material;
    }

    // Функция, которая выполняется на GPU.
    private static void ExecutePass(PassData data, RasterGraphContext context)
    {
        // Устанавливаем исходную текстуру в материал.
        data.material.SetTexture("_MainTex", data.sourceTexture);
        // Рисуем полноэкранный треугольник.
        Blitter.BlitTexture(context.cmd, data.sourceTexture, Vector4.one, data.material, 0);
    }
}
