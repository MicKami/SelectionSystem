using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.Rendering.Universal.Internal;

public class SelectableIDMapRendererFeature : ScriptableRendererFeature
{
	class SelectableIDMapRenderPass : ScriptableRenderPass
	{
		private List<ShaderTagId> shaderTagsList = new();
		private FilteringSettings filteringSettings;
		private RTHandle selectablesID;
		private RTHandle depth;
		private readonly int downscaleFactor;

		public SelectableIDMapRenderPass(int layer, string name, int downscaleFactor)
		{
			filteringSettings = new FilteringSettings(RenderQueueRange.opaque, layer);

			shaderTagsList.Add(new ShaderTagId("Unlit"));

			profilingSampler = new ProfilingSampler(name);
			this.downscaleFactor = downscaleFactor;
			
		}

		public void Setup(RTHandle depth)
		{
			this.depth = depth;
		}

		public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
		{
			var descriptor = renderingData.cameraData.cameraTargetDescriptor;
			descriptor.width >>= downscaleFactor;
			descriptor.height >>= downscaleFactor;

			descriptor.msaaSamples = 1;
			descriptor.graphicsFormat = UnityEngine.Experimental.Rendering.GraphicsFormat.R8G8B8A8_SRGB;
			descriptor.autoGenerateMips = false;
			descriptor.depthBufferBits = 0;
			descriptor.enableRandomWrite = true;
			RenderingUtils.ReAllocateIfNeeded(ref selectablesID, descriptor, FilterMode.Point);

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

				SelectableIDMapSampler.SelectableIDMap = selectablesID.rt;
				SelectableIDMapSampler.DownscaleFactor = downscaleFactor;
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
	SelectableIDMapRenderPass m_ScriptablePass;
	DepthOnlyPass depthOnlyPass;
	RTHandle depth;
	public override void Create()
	{
		m_ScriptablePass = new SelectableIDMapRenderPass(layer, name, downscaleFactor);
		m_ScriptablePass.renderPassEvent = RenderPassEvent.AfterRenderingPostProcessing;

		depthOnlyPass = new DepthOnlyPass(RenderPassEvent.BeforeRenderingPostProcessing, RenderQueueRange.opaque, ~0);
	}
	public override void SetupRenderPasses(ScriptableRenderer renderer, in RenderingData renderingData)
	{
		var descriptor = renderingData.cameraData.cameraTargetDescriptor;
		descriptor.msaaSamples = 1;
		descriptor.width >>= downscaleFactor;
		descriptor.height >>= downscaleFactor;
		descriptor.depthBufferBits = 32;

		RenderingUtils.ReAllocateIfNeeded(ref depth, descriptor);

		depthOnlyPass.Setup(descriptor, depth); 
		m_ScriptablePass.Setup(depth);
	}

	public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
	{
		CameraType cameraType = renderingData.cameraData.cameraType;
		if (cameraType == CameraType.Preview) return;
		if (cameraType == CameraType.SceneView) return;

		renderer.EnqueuePass(depthOnlyPass); 
		renderer.EnqueuePass(m_ScriptablePass);
	}
}


