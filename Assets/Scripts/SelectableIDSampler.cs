using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class SelectableIDSampler : MonoBehaviour
{
    public static RenderTexture IDMap;
    private Action<HashSet<uint>> sampleRegionCallback;
    private int downscaleFactor = 0;
    private ComputeShader computeShader;
    private ComputeBuffer outputBuffer;
    private int initializedKernelID;
    private int mainKernelID;

    private void Awake()
    {
        computeShader = Resources.Load<ComputeShader>("ComputeShaders/ReadRenderTextureIDs");
        initializedKernelID = computeShader.FindKernel("Initialize");
        mainKernelID = computeShader.FindKernel("Main");
    }

    public (int x, int y) DownScale(Vector2 value)
    {
        for (int i = 0; i < downscaleFactor; i++)
        {
            value /= 2;
        }
        return (Mathf.CeilToInt(value.x), Mathf.CeilToInt(value.y));
    }

    public void SampleAtPosition(Vector2 position, Action<HashSet<uint>> callback)
    {
        if (IDMap)
        {
            sampleRegionCallback = callback;
            (int x, int y) = DownScale(position);
            AsyncGPUReadback.Request(IDMap, 0, x, 1, y, 1, 0, 1, SamplePoint);
        }
    }

    private void SamplePoint(AsyncGPUReadbackRequest request)
    {
        var data = request.GetData<Color32>();
        uint id = SelectionUtility.ColorToID(data[0]);
        sampleRegionCallback(new HashSet<uint>(new uint[] { id }));
    }

    public void Sample(Rect region, Action<HashSet<uint>> callback)
    {
        if (region.width >= 1 && region.height >= 1)
        {
            SampleAtRegion(region, callback);
        }
        else SampleAtPosition(region.position, callback);
            
        IDMap?.Release();
    }

    public void SampleAtRegion(Rect region, Action<HashSet<uint>> callback)
    {
        if (IDMap)
        {
            (int x, int y) = DownScale(region.position);
            (int width, int height) = DownScale(region.size);
            region = new Rect(x, y, width, height);
            sampleRegionCallback = callback;
            outputBuffer = new ComputeBuffer(Selection.Selectables.Count + 1, sizeof(uint));

            computeShader.SetBuffer(initializedKernelID, "Output", outputBuffer);
            computeShader.SetBuffer(mainKernelID, "Output", outputBuffer);
            computeShader.SetTextureFromGlobal(mainKernelID, "_SelectablesID", "_SelectablesID");
            computeShader.SetVector("Rect", new Vector4(region.x, region.y, region.width, region.height));
            computeShader.Dispatch(initializedKernelID, Mathf.CeilToInt(Selection.Selectables.Count + 1 / 64f), 1, 1);

            var (threadGroupsX, threadGroupsY) = (Mathf.CeilToInt(region.width / 8f), Mathf.CeilToInt(region.height / 8f));
            computeShader.Dispatch(mainKernelID, threadGroupsX, threadGroupsY, 1);
            AsyncGPUReadback.Request(outputBuffer, SampleRegion);
            outputBuffer.Dispose();
        }
    }

    private void SampleRegion(AsyncGPUReadbackRequest request)
    {
        var result = request.GetData<uint>();
		HashSet<uint> ids = new();

        for (uint i = 1; i < result.Length; i++)
        {
            if (result[(int)i] > 0)
            {
                ids.Add(i);
            }
        }
        sampleRegionCallback(ids);
    }
}
