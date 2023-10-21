using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public static class SelectablesSampler
{
	public static RenderTexture IDMap { get; set; }
	public static int DownscaleFactor { get; set; }

	private static ComputeBuffer outputBuffer;
	private static ComputeShader computeShader;
	private static int initializedKernelID;
	private static int mainKernelID;

	[RuntimeInitializeOnLoadMethod]
	private static void Initialize()
	{
		computeShader = Resources.Load<ComputeShader>("ComputeShaders/ReadRenderTextureIDs");
		initializedKernelID = computeShader.FindKernel("Initialize");
		mainKernelID = computeShader.FindKernel("Main");
	}

	public static (int x, int y) DownScale(Vector2 value)
	{
		for (int i = 0; i < DownscaleFactor; i++)
		{
			value /= 2;
		}
		return (Mathf.CeilToInt(value.x), Mathf.CeilToInt(value.y));
	}

	public static async Awaitable<uint> SampleAtPosition(Vector2 position)
	{
		(int x, int y) = DownScale(position);
		var request = await AsyncGPUReadback.RequestAsync(IDMap, 0, x, 1, y, 1, 0, 1);
		var data = request.GetData<Color32>();
		uint id = SelectionUtility.ColorToID(data[0]);
		return id;

	}
	public static async Awaitable<IEnumerable<uint>> Sample(Rect region)
	{
		if (IDMap)
		{
			if (region.width >= 1 && region.height >= 1)
			{
				return await SampleAtRegion(region);
			}
			return new uint[] { await SampleAtPosition(region.position) };
		}
		return new uint[] {};
	}

	public static async Awaitable<IEnumerable<uint>> SampleAtRegion(Rect region)
	{
		(int x, int y) = DownScale(region.position);
		(int width, int height) = DownScale(region.size);
		region = new Rect(x, y, width, height);
		outputBuffer?.Dispose();
		outputBuffer = new ComputeBuffer(Selection.Selectables.Count + 1, sizeof(uint));

		computeShader.SetBuffer(initializedKernelID, "Output", outputBuffer);
		computeShader.SetBuffer(mainKernelID, "Output", outputBuffer);
		computeShader.SetTextureFromGlobal(mainKernelID, "_SelectablesID", "_SelectablesID");
		computeShader.SetVector("Rect", new Vector4(region.x, region.y, region.width, region.height));
		computeShader.Dispatch(initializedKernelID, Mathf.CeilToInt(Selection.Selectables.Count + 1 / 64f), 1, 1);
		var (threadGroupsX, threadGroupsY) = (Mathf.CeilToInt(region.width / 8f), Mathf.CeilToInt(region.height / 8f));
		computeShader.Dispatch(mainKernelID, threadGroupsX, threadGroupsY, 1);

		var request = await AsyncGPUReadback.RequestAsync(outputBuffer);
		var result = request.GetData<uint>();
		outputBuffer?.Dispose();

		List<uint> ids = new();
		for (uint i = 1; i < result.Length; i++)
		{
			if (result[(int)i] > 0)
			{
				ids.Add(i);
			}
		}
		return ids;
	}
}
