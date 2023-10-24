using UnityEngine;
using UnityEngine.Rendering;

public class ShowSelectionMask : MonoBehaviour
{
	private Material material;
	[field: SerializeField]
	public bool Enabled { get; set; }
	private void Awake()
	{
		material = new Material(Shader.Find("Hidden/ShowSelectionMask"));
	}
	private void OnEnable()
	{
		RenderPipelineManager.endCameraRendering += RenderPipelineManager_endCameraRendering;
	}
	private void OnDisable()
	{
		RenderPipelineManager.endCameraRendering -= RenderPipelineManager_endCameraRendering;
	}
	private void RenderPipelineManager_endCameraRendering(ScriptableRenderContext context, Camera camera)
	{		
		if(!Enabled) return;

		var cmd = CommandBufferPool.Get();
		cmd.Blit(BuiltinRenderTextureType.CurrentActive, BuiltinRenderTextureType.CurrentActive, material);
		context.ExecuteCommandBuffer(cmd);
		context.Submit();
		CommandBufferPool.Release(cmd);
	}
	private void Update()
	{
		if(Input.GetKeyDown(KeyCode.Space))
		{
			Enabled = !Enabled;
		}	
	}
}
