using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting.Dependencies.Sqlite;
using UnityEditor;
using UnityEditor.PackageManager.UI;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

public class SelectableIDMap : MonoBehaviour
{
    [Range(0, 2)]
    public int downscaleFactor;

    private RenderTexture IDMap;
    private Material selectableID;
    private MaterialPropertyBlock materialPropertyBlock;

    private Action<uint> sampleAtPositionCallback;
    private Action<List<uint>> sampleAtRegionCallback;

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

    public void SampleAtPosition(Vector2 position, Action<uint> callback)
    {
        if (IDMap)
        {
            sampleAtPositionCallback = callback;
            (int x, int y) = ((int)position.x >> downscaleFactor, (int)position.y >> downscaleFactor);
            AsyncGPUReadback.Request(IDMap, 0, x, 1, y, 1, 0, 1, SamplePoint);
        }
    }

    private void SamplePoint(AsyncGPUReadbackRequest request)
    {
        var data = request.GetData<Color32>();
        uint id = SelectionUtility.ColorToID(data[0]);
        sampleAtPositionCallback(id);
    }

    public void SampleAtRegion(Rect region, Action<List<uint>> callback)
    {
        if (IDMap)
        {
            sampleAtRegionCallback = callback;
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
        sampleAtRegionCallback(ids);
    }

    private void OnDrawGizmos()
    {
        if (!EditorApplication.isPlaying) return;
        Gizmos.color = Color.magenta;
        foreach (var hover in Selection.Hover)
        {
            Gizmos.DrawWireCube(hover.transform.position, hover.Renderer.bounds.size);
        }
        Gizmos.color = Color.white;
        foreach (var selection in Selection.Active)
        {
            Gizmos.DrawWireCube(selection.transform.position, selection.Renderer.bounds.size);
        }
    }
}
