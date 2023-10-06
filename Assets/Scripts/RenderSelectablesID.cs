using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class RenderSelectablesID : ScriptableRendererFeature
{
    class CustomRenderPass : ScriptableRenderPass
    {
        private List<ShaderTagId> shaderTagsList = new();
        private FilteringSettings filteringSettings;
        private RTHandle selectablesID;
        public CustomRenderPass(int layer, string name)
        {
            filteringSettings = new FilteringSettings(RenderQueueRange.opaque, layer);

            shaderTagsList.Add(new ShaderTagId("Unlit"));

            profilingSampler = new ProfilingSampler(name);
        }

        public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
        {
            var colorDesc = renderingData.cameraData.cameraTargetDescriptor;
            colorDesc.msaaSamples = 1;
            colorDesc.graphicsFormat = UnityEngine.Experimental.Rendering.GraphicsFormat.R8G8B8A8_SRGB;
            colorDesc.depthBufferBits = 0;
            colorDesc.autoGenerateMips = false;

            RenderingUtils.ReAllocateIfNeeded(ref selectablesID, colorDesc, FilterMode.Point);

            RTHandle depth = renderingData.cameraData.renderer.cameraDepthTargetHandle;
            ConfigureTarget(selectablesID, depth);
            ConfigureClear(ClearFlag.Color, Color.clear);

        }

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            var cmd = CommandBufferPool.Get();
            using (new ProfilingScope(cmd, profilingSampler))
            {
                context.ExecuteCommandBuffer(cmd);
                cmd.Clear();

                SortingCriteria sortingCriteria = SortingCriteria.CommonOpaque;
                DrawingSettings drawingSettings = CreateDrawingSettings(shaderTagsList, ref renderingData, sortingCriteria);
                RendererListParams rendererListParams = new RendererListParams(renderingData.cullResults, drawingSettings, filteringSettings);
                RendererList rendererList = context.CreateRendererList(ref rendererListParams);
                cmd.DrawRendererList(rendererList);

                SelectableIDSampler.IDMap = selectablesID.rt;
                Shader.SetGlobalTexture("_SelectablesID", selectablesID.rt);


            }
            context.ExecuteCommandBuffer(cmd);
            cmd.Clear();
            CommandBufferPool.Release(cmd);
        }
    }

    public LayerMask layer;
    CustomRenderPass m_ScriptablePass;

    public override void Create()
    {
        m_ScriptablePass = new CustomRenderPass(layer, name);
        m_ScriptablePass.renderPassEvent = RenderPassEvent.AfterRenderingPostProcessing;
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        CameraType cameraType = renderingData.cameraData.cameraType;
        if (cameraType == CameraType.Preview) return;
        if (cameraType == CameraType.SceneView) return;
        renderer.EnqueuePass(m_ScriptablePass);
    }
}


