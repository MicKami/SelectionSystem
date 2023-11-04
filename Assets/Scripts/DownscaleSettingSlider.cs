using UnityEngine;
using UnityEngine.Rendering.Universal;
using TMPro;
using UnityEngine.UI;

public class DownscaleSettingSlider : MonoBehaviour
{
	[SerializeField]
	private UniversalRendererData rendererData;
	[SerializeField]
	private TextMeshProUGUI text;
	[SerializeField]
	private Slider slider;

	private SelectableIDMapRendererFeature renderFeature;

	private void Awake()
	{
		renderFeature = rendererData.rendererFeatures.Find(x => x is SelectableIDMapRendererFeature) as SelectableIDMapRendererFeature;
		slider.value = renderFeature.downscaleFactor;
	}

	public void SetDownscaleFactor(float factor)
	{
		text.text = factor.ToString();
		renderFeature.downscaleFactor = (int)factor;
		rendererData.SetDirty();
	}
}
