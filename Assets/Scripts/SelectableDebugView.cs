using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

public class SelectableDebugView : MonoBehaviour
{
	public static bool Enabled { get; set; }

	[SerializeField]
	private Toggle UI_Toggle;
	private Material material;

	private void Awake()
	{
		material = new Material(Shader.Find("Hidden/SelectableDebugView"));
		UI_Toggle.isOn = Enabled;
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

		var cmd = CommandBufferPool.Get("SelectableDebugView");
		cmd.Blit(BuiltinRenderTextureType.CurrentActive, BuiltinRenderTextureType.CurrentActive, material);
		context.ExecuteCommandBuffer(cmd);
		context.Submit();
		CommandBufferPool.Release(cmd);
	}
	private void Update()
	{
		if(Input.GetKeyDown(KeyCode.Space))
		{
			UI_Toggle.isOn = !UI_Toggle.isOn;
		}
	}
}
