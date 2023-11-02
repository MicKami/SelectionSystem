using UnityEngine;
using UnityEngine.Rendering.Universal;
using TMPro;
using UnityEngine.UI;

public class DownscaleSettingSlider : MonoBehaviour
{
	[SerializeField] UniversalRendererData rendererData;
	[SerializeField] TextMeshProUGUI text;	
	[SerializeField] Slider slider;
	RenderSelectablesID renderFeature;

	private void Awake()
	{
		renderFeature = rendererData.rendererFeatures.Find(x => x is RenderSelectablesID) as RenderSelectablesID;
		slider.value = renderFeature.downscaleFactor;
	}  

	public void SetDownscaleFactor(float factor)
	{
		text.text = factor.ToString();
		renderFeature.downscaleFactor = (int)factor;
		rendererData.SetDirty();
	}
}
