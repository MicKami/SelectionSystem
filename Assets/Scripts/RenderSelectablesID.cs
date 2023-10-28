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
		private RTHandle depthCopy;
		private readonly int downscaleFactor;

		public CustomRenderPass(int layer, string name, int downscaleFactor)
		{
			filteringSettings = new FilteringSettings(RenderQueueRange.opaque, layer);

			shaderTagsList.Add(new ShaderTagId("Unlit"));

			profilingSampler = new ProfilingSampler(name);
			this.downscaleFactor = downscaleFactor;
		}

		public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
		{
			var desc = renderingData.cameraData.cameraTargetDescriptor;
			for (int i = 0; i < downscaleFactor; i++)
			{
				desc.width /= 2;
				desc.height /= 2;
			}
			var depth = renderingData.cameraData.renderer.cameraDepthTargetHandle;
			RenderingUtils.ReAllocateIfNeeded(ref depthCopy, desc);
			Blitter.BlitCameraTexture(cmd, depth, depthCopy);
			desc.msaaSamples = 1;
			desc.graphicsFormat = UnityEngine.Experimental.Rendering.GraphicsFormat.R8G8B8A8_SRGB;
			desc.autoGenerateMips = false;
			desc.depthBufferBits = 0;
			desc.enableRandomWrite = true;
			RenderingUtils.ReAllocateIfNeeded(ref selectablesID, desc, FilterMode.Point);
			ConfigureTarget(selectablesID, depthCopy);
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

				SelectablesSampler.IDMap = selectablesID.rt;
				SelectablesSampler.DownscaleFactor = downscaleFactor;
				Shader.SetGlobalTexture("_SelectablesID", selectablesID.rt);


			}
			context.ExecuteCommandBuffer(cmd);
			cmd.Clear();
			CommandBufferPool.Release(cmd);
		}
		
	}

	public LayerMask layer;
	[Range(0, 2)]
	public int downscaleFactor;
	CustomRenderPass m_ScriptablePass;

	public override void Create()
	{
		m_ScriptablePass = new CustomRenderPass(layer, name, downscaleFactor);
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


