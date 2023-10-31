using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using System.Reflection;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class DownscaleSettingSlider : MonoBehaviour
{
	[SerializeField] UniversalRendererData rendererData;
	RenderSelectablesID renderFeature;

	private void Awake()
	{
		renderFeature = rendererData.rendererFeatures.Find(x => x is RenderSelectablesID) as RenderSelectablesID;
	}

	public void SetDownscaleFactor(float factor)
	{
		renderFeature.downscaleFactor = (int)factor;
		rendererData.SetDirty();
	}
	private void OnDestroy()
	{
		SetDownscaleFactor(0);
	}
}
