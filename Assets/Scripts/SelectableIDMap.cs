using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UIElements;

public class SelectableIDMap : MonoBehaviour
{
    [Range(0, 2)]
    public int downscaleFactor;

    private RenderTexture IDMap;
    private Material selectableID;
    private MaterialPropertyBlock materialPropertyBlock;

    private Action<IEnumerable<uint>> sampleRegionCallback;

    private ComputeShader computeShader;
    private int initializedKernelID;
    private int mainKernelID;
    private ComputeBuffer outputBuffer;

    private void Awake()
    {
        selectableID = new Material(Shader.Find("Custom/SelectableID"));
        materialPropertyBlock = new MaterialPropertyBlock();

        computeShader = Resources.Load<ComputeShader>("ComputeShaders/ReadRenderTextureIDs");
        initializedKernelID = computeShader.FindKernel("Initialize");
        mainKernelID = computeShader.FindKernel("Main");
    }

    private void OnEnable()
    {
        RenderPipelineManager.endCameraRendering += OnEndCameraRendering;
    }

    private void OnEndCameraRendering(ScriptableRenderContext context, Camera camera)
    {
        if (camera.cameraType != CameraType.Game) return;

        CommandBuffer cmd = CommandBufferPool.Get("SelectableID");
        IDMap = RenderTexture.GetTemporary(Screen.width >> downscaleFactor, Screen.height >> downscaleFactor, 24);
        IDMap.autoGenerateMips = false;
        IDMap.antiAliasing = 1;
        IDMap.filterMode = FilterMode.Point;
        cmd.SetRenderTarget(IDMap);
        cmd.ClearRenderTarget(false, true, Color.clear);

        foreach (var selectable in Selection.Selectables)
        {
            var renderer = selectable.Renderer;
            materialPropertyBlock.Clear();
            materialPropertyBlock.SetColor("_Color", selectable.Color32);
            renderer.SetPropertyBlock(materialPropertyBlock);
            cmd.DrawRenderer(renderer, selectableID);
        }
        context.ExecuteCommandBuffer(cmd);
        context.Submit();

        //reset renderer property block to make SRP Batcher work
        foreach (var selectable in Selection.Selectables)
        {
            selectable.Renderer.SetPropertyBlock(null);
        }

        if (SystemInfo.graphicsUVStartsAtTop)
        {
            VerticallyFlipRenderTexture(IDMap);
        }

        cmd.Clear();
        cmd.SetRenderTarget(BuiltinRenderTextureType.CameraTarget);
        context.ExecuteCommandBuffer(cmd);
        context.Submit();
        CommandBufferPool.Release(cmd);
        RenderTexture.ReleaseTemporary(IDMap);
    }
    private void OnDisable()
    {
        RenderPipelineManager.endCameraRendering -= OnEndCameraRendering;
    }
    private void VerticallyFlipRenderTexture(RenderTexture renderTexture)
    {
        var temp = RenderTexture.GetTemporary(renderTexture.descriptor);
        Graphics.Blit(renderTexture, temp, new Vector2(1, -1), new Vector2(0, 1));
        Graphics.Blit(temp, renderTexture);
        RenderTexture.ReleaseTemporary(temp);
    }

    public (int x, int y) DownScale(Vector2 value)
    {
        for (int i = 0; i < downscaleFactor; i++)
        {
            value /= 2;
        }
        return (Mathf.CeilToInt(value.x), Mathf.CeilToInt(value.y));
    }

    public void SampleAtPosition(Vector2 position, Action<IEnumerable<uint>> callback)
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
        sampleRegionCallback(new uint[] { id });
    }

    public void Sample(Rect region, Action<IEnumerable<uint>> callback)
    {
        print(region);
        if (region.width >= 1 && region.height >= 1)
        {
            SampleAtRegion(region, callback);
        }
        else SampleAtPosition(region.position, callback);
    }

    public void SampleAtRegion(Rect region, Action<IEnumerable<uint>> callback)
    {
        if (IDMap)
        {
            (int x, int y) = DownScale(region.position);
            (int width, int height) = DownScale(region.size);
            region = new Rect(x, y, width, height);
            sampleRegionCallback = callback;
            outputBuffer = new ComputeBuffer((int)SelectableBase.IDsCount + 1, sizeof(uint));

            computeShader.SetBuffer(initializedKernelID, "Output", outputBuffer);
            computeShader.SetBuffer(mainKernelID, "Output", outputBuffer);
            computeShader.SetTexture(mainKernelID, "Input", IDMap);
            computeShader.SetVector("Rect", new Vector4(region.x, region.y, region.width, region.height));
            computeShader.Dispatch(initializedKernelID, Mathf.CeilToInt((int)SelectableBase.IDsCount / 64f), 1, 1);

            var (threadGroupsX, threadGroupsY) = (Mathf.CeilToInt(region.width / 8f), Mathf.CeilToInt(region.height / 8f));
            computeShader.Dispatch(mainKernelID, threadGroupsX, threadGroupsY, 1);
            AsyncGPUReadback.Request(outputBuffer, SampleRegion);
            outputBuffer.Dispose();
        }
    }

    private void SampleRegion(AsyncGPUReadbackRequest request)
    {
        var result = request.GetData<uint>();
        List<uint> ids = new();

        for (uint i = 1; i < result.Length; i++)
        {
            if (result[(int)i] > 0)
            {
                ids.Add(i);
            }
        }
        sampleRegionCallback(ids);
    }

    private void OnDrawGizmos()
    {
        if (!EditorApplication.isPlaying) return;
        Gizmos.color = Color.magenta;
        foreach (var hover in Selection.Hover)
        {
            Gizmos.DrawWireCube(hover.transform.position, hover.Renderer.bounds.size);
        }
    }
}
