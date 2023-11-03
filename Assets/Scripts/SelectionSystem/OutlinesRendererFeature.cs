using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class OutlinesRendererFeature : ScriptableRendererFeature
{
    class OutlinesRenderPass : ScriptableRenderPass
    {
        private Settings settings;
        private FilteringSettings filteringSettings;
        private List<ShaderTagId> shaderTagsList = new ();
        private RTHandle outlinesMaskRT;
        private RTHandle outlinesDilatedRT;
        private RTHandle tempRT;

        public OutlinesRenderPass(Settings settings, string name)
        {
            this.settings = settings;
            filteringSettings = new FilteringSettings(RenderQueueRange.opaque, settings.layerMask);

            shaderTagsList.Add(new ShaderTagId("SRPDefaultUnlit"));
            shaderTagsList.Add(new ShaderTagId("UniversalForward"));
            shaderTagsList.Add(new ShaderTagId("UniversalForwardOnly"));

            profilingSampler = new ProfilingSampler(name);
        }

        public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
        {
            var colorDesc = renderingData.cameraData.cameraTargetDescriptor;
            colorDesc.depthBufferBits = 0;
            colorDesc.colorFormat = RenderTextureFormat.ARGB32;
            colorDesc.msaaSamples = 1;

            RenderingUtils.ReAllocateIfNeeded(ref outlinesMaskRT, colorDesc);
            RenderingUtils.ReAllocateIfNeeded(ref outlinesDilatedRT, colorDesc);
            RenderingUtils.ReAllocateIfNeeded(ref tempRT, colorDesc);

            ConfigureTarget(outlinesMaskRT);
            ConfigureClear(ClearFlag.Color, Color.clear);
        }

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            CommandBuffer cmd = CommandBufferPool.Get();
            using (new ProfilingScope(cmd, profilingSampler))
            {
                context.ExecuteCommandBuffer(cmd);
                cmd.Clear();

                var sortingCriteria = renderingData.cameraData.defaultOpaqueSortFlags;

                var drawingSettings = CreateDrawingSettings(shaderTagsList, ref renderingData, sortingCriteria);
                drawingSettings.overrideMaterialPassIndex = 0;
                drawingSettings.overrideMaterial = settings.overrideMaterial;

                var rendererListParams = new RendererListParams(renderingData.cullResults, drawingSettings, filteringSettings);

                var renderList = context.CreateRendererList(ref rendererListParams);
                cmd.DrawRendererList(renderList);


                settings.dilateMaterial.SetFloat("_Diagonal", settings.diagonal ? 1 : 0);

                Blitter.BlitCameraTexture(cmd, outlinesMaskRT, outlinesDilatedRT, settings.dilateMaterial, 0);

                for (int i = 0; i < settings.steps - 1; i++)
                {
                    Blitter.BlitCameraTexture(cmd, outlinesDilatedRT, tempRT, settings.dilateMaterial, 0);
                    (tempRT, outlinesDilatedRT) = (outlinesDilatedRT, tempRT);
                }
                cmd.SetGlobalTexture("_OutlineMask", outlinesMaskRT);
                cmd.SetGlobalTexture("_OutlineDilated", outlinesDilatedRT);
                settings.blitMaterial.color = settings.color;
                RTHandle screenBuffer = renderingData.cameraData.renderer.cameraColorTargetHandle;
                Blitter.BlitCameraTexture(cmd, screenBuffer, screenBuffer, settings.blitMaterial, 0);
                
            }
            context.ExecuteCommandBuffer(cmd);
            cmd.Clear();
            CommandBufferPool.Release(cmd);
        }

        public override void OnCameraCleanup(CommandBuffer cmd) 
        {
            outlinesMaskRT?.Release();
            outlinesDilatedRT?.Release();
            tempRT?.Release();
        }
    }


    [System.Serializable]
    public class Settings
    {
        public LayerMask layerMask = 1;
        [HideInInspector] public Material overrideMaterial;
        [Range(1, 4)]
        public int steps;
        public bool diagonal;
        public Color color;
        [HideInInspector] public Material dilateMaterial;
        [HideInInspector] public Material blitMaterial;
    }

    public Settings settings = new Settings();
    private OutlinesRenderPass m_ScriptablePass;

    public override void Create()
    {
        settings.overrideMaterial = CoreUtils.CreateEngineMaterial("Universal Render Pipeline/Unlit");
        settings.dilateMaterial = CoreUtils.CreateEngineMaterial("Hidden/Dilate");
        settings.blitMaterial = CoreUtils.CreateEngineMaterial("Hidden/Final Blit");
        m_ScriptablePass = new OutlinesRenderPass(settings, name);
        m_ScriptablePass.renderPassEvent = RenderPassEvent.BeforeRenderingPostProcessing;
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        CameraType cameraType = renderingData.cameraData.cameraType;
        if (cameraType == CameraType.Preview) return;
        if (cameraType == CameraType.SceneView) return;
        renderer.EnqueuePass(m_ScriptablePass);
    }

    protected override void Dispose(bool disposing)
    {
        CoreUtils.Destroy(settings.overrideMaterial);
        CoreUtils.Destroy(settings.dilateMaterial);
        CoreUtils.Destroy(settings.blitMaterial);
    }
}

