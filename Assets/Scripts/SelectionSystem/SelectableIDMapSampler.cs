using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public static class SelectableIDMapSampler
{
	public static RenderTexture SelectableIDMap { get; set; }
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
	private static (int x, int y) Downscale(Vector2 value)
	{
		for (int i = 0; i < DownscaleFactor; i++)
		{
			value /= 2;
		}
		return (Mathf.FloorToInt(value.x), Mathf.FloorToInt(value.y));
	}
	private static Vector2 FixRounding(Vector2 position)
	{
		if (DownscaleFactor > 0)
		{
			if (Screen.width % 2 == 1)
			{
				position.x *= 1 - (1.0f / (Screen.width + 1));
				position.x = Mathf.Floor(position.x + 0.5f);
			}
			if (Screen.height % 2 == 1)
			{
				position.y *= 1 - (1.0f / (Screen.height + 1));
				position.y = Mathf.Floor(position.y + 0.5f);
			}
		}
		return position;
	}
	public static async Awaitable<IEnumerable<uint>> Sample(Rect region)
	{
		if (SelectableIDMap)
		{
			if (region.width > 1 || region.height > 1)
			{
				return await SampleAtRegion(region);
			}
			return new uint[] { await SampleAtPosition(region.position) };
		}
		return new uint[] { };
	}
	public static async Awaitable<uint> SampleAtPosition(Vector2 position)
	{
		(int x, int y) = Downscale(FixRounding(position));
		y = Mathf.Clamp(y, 0, (Screen.height - (1 << DownscaleFactor)) >> DownscaleFactor);
		x = Mathf.Clamp(x, 0, (Screen.width - (1 << DownscaleFactor)) >> DownscaleFactor);
		var request = await AsyncGPUReadback.RequestAsync(SelectableIDMap, 0, x, 1, y, 1, 0, 1);
		var data = request.GetData<Color32>();
		return SelectionUtility.ColorToID(data[0]);
	}
	public static async Awaitable<IEnumerable<uint>> SampleAtRegion(Rect region)
	{
		(int x1, int y1) = Downscale(FixRounding(region.min));
		Vector2 scaledPixelOffset = Vector2.one * ((1 << DownscaleFactor) - 1);
		(int x2, int y2) = Downscale(FixRounding(region.max) + scaledPixelOffset);
		(int width, int height) = (x2 - x1, y2 - y1);
		var scaledRegion = new Rect(x1, y1, width, height);

		outputBuffer?.Dispose();
		outputBuffer = new ComputeBuffer(Selection.Selectables.Count + 1, sizeof(uint));

		computeShader.SetBuffer(initializedKernelID, "Output", outputBuffer);
		computeShader.SetBuffer(mainKernelID, "Output", outputBuffer);
		computeShader.SetTextureFromGlobal(mainKernelID, "_SelectablesID", "_SelectablesID");
		computeShader.SetVector("Rect", new Vector4(scaledRegion.x, scaledRegion.y, scaledRegion.width, scaledRegion.height));
		computeShader.Dispatch(initializedKernelID, Mathf.CeilToInt(Selection.Selectables.Count + 1 / 64f), 1, 1);
		var (threadGroupsX, threadGroupsY) = (Mathf.CeilToInt((scaledRegion.width + 1) / 8f), Mathf.CeilToInt((scaledRegion.height + 1) / 8f));
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
