using System;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

public class SelectionManager : MonoBehaviour
{
    public Material selectableID;
    public Material selectionUnlit;
    public Material composite;
    public JumpFlood jumpFlood;

    [Range(0, 2)]
    public int downscaleFactor;

    private RenderTexture selectableRT;

    private bool isDragging;
    private Vector2 dragBeginPosition;
    private Rect selectionRect;

    public ComputeShader computeShader;
    int initializedKernelID;
    int mainKernelID;
    ComputeBuffer outputBuffer;

    MaterialPropertyBlock materialPropertyBlock;

    private void OnEnable()
    {
        materialPropertyBlock = new MaterialPropertyBlock();
        RenderPipelineManager.endCameraRendering += OnEndCameraRendering;

        initializedKernelID = computeShader.FindKernel("Initialize");
        mainKernelID = computeShader.FindKernel("Main");
    }

    private void OnEndCameraRendering(ScriptableRenderContext context, Camera camera)
    {
        if (camera.cameraType != CameraType.Game) return;

        CommandBuffer cmd = CommandBufferPool.Get("SelectableID");
        selectableRT = RenderTexture.GetTemporary(Screen.width >> downscaleFactor, Screen.height >> downscaleFactor, 24);
        selectableRT.autoGenerateMips = false;
        selectableRT.antiAliasing = 1;
        selectableRT.filterMode = FilterMode.Point;
        cmd.SetRenderTarget(selectableRT);
        cmd.ClearRenderTarget(false, true, Color.clear);

        foreach (var selectable in Selection.Selectables)
        {
            var renderer = selectable.Renderer;
            materialPropertyBlock.Clear();
            materialPropertyBlock.SetColor("_Color", selectable.Color32);
            renderer.SetPropertyBlock(materialPropertyBlock);
            cmd.DrawRenderer(renderer, selectableID);
        }
        //cmd.Blit(selectableRT, BuiltinRenderTextureType.CameraTarget);
        context.ExecuteCommandBuffer(cmd);
        context.Submit();

        foreach (var selectable in Selection.Selectables)
        {
            var renderer = selectable.Renderer;
            renderer.SetPropertyBlock(null);
        }

        if (SystemInfo.graphicsUVStartsAtTop)
        {
            VerticallyFlipRenderTexture(selectableRT);
        }

        cmd.Clear();
        cmd.SetRenderTarget(BuiltinRenderTextureType.CameraTarget);
        context.ExecuteCommandBuffer(cmd);
        context.Submit();

        //if (selectionData.CurrentSelection.Count > 0)
        //{
        //    cmd.Clear();
        //    var selectionRT = RenderTexture.GetTemporary(Screen.width, Screen.height, 0, RenderTextureFormat.ARGBFloat, RenderTextureReadWrite.Linear, QualitySettings.antiAliasing);
        //    cmd.SetRenderTarget(selectionRT);
        //    cmd.ClearRenderTarget(false, true, Color.clear);
        //    foreach (var item in selectionData.CurrentSelection)
        //    {
        //        var selectionRenderer = item.Renderer;
        //        cmd.DrawRenderer(selectionRenderer, selectionUnlit, 0, 0);
        //    }
        //    context.ExecuteCommandBuffer(cmd);
        //    context.Submit();

        //    cmd.Clear();

        //    var JFA = jumpFlood.BuildDistanceField(selectionRT);
        //    Shader.SetGlobalTexture("_TargetTexture", selectionRT);
        //    composite.SetFloat("_OutlineWidth", 0.0035f);
        //    composite.SetColor("_OutlineColor", Color.cyan);

        //    cmd.Blit(JFA, BuiltinRenderTextureType.CameraTarget, composite);
        //    RenderTexture.ReleaseTemporary(selectionRT);
        //    RenderTexture.ReleaseTemporary(JFA);
        //    context.ExecuteCommandBuffer(cmd);
        //    context.Submit();
        //}

        //if (selectionData.HoverSelection.Count > 0)
        //{
        //    cmd.Clear();
        //    var selectionRT = RenderTexture.GetTemporary(Screen.width, Screen.height, 0, RenderTextureFormat.ARGBFloat, RenderTextureReadWrite.Linear, QualitySettings.antiAliasing);
        //    cmd.SetRenderTarget(selectionRT);
        //    cmd.ClearRenderTarget(false, true, Color.clear);

        //    foreach (var item in selectionData.HoverSelection)
        //    {
        //        var selectionRenderer = item.Renderer
        //        cmd.DrawRenderer(selectionRenderer, selectionUnlit, 0, 0);
        //    }
        //    context.ExecuteCommandBuffer(cmd);
        //    context.Submit();

        //    cmd.Clear();

        //    var JFA = jumpFlood.BuildDistanceField(selectionRT);
        //    Shader.SetGlobalTexture("_TargetTexture", selectionRT);
        //    composite.SetFloat("_OutlineWidth", 0.0035f);
        //    composite.SetColor("_OutlineColor", Color.magenta);

        //    cmd.Blit(JFA, BuiltinRenderTextureType.CameraTarget, composite);
        //    RenderTexture.ReleaseTemporary(selectionRT);
        //    RenderTexture.ReleaseTemporary(JFA);
        //    context.ExecuteCommandBuffer(cmd);
        //    context.Submit();
        //}

        CommandBufferPool.Release(cmd);
        RenderTexture.ReleaseTemporary(selectableRT);
    }

    private void Update()
    {
        if (!selectableRT) return;
        if (!isDragging)
        {
            var mousePosition = MousePosition();
            if (IsPositionWithinScreen(mousePosition))
            {
                SampleRenderTextureAtPosition(MousePosition(), selectableRT, request =>
                {
                    var data = request.GetData<Color32>();
                    uint id = SelectionUtility.ColorToID(data[0]);
                    Selection.SetHover(id);
                    if (Input.GetMouseButtonDown(0))
                    {
                        Selection.Set(id);
                    }
                    data.Dispose();
                }
                );
            }
        }

        if (Input.GetMouseButtonDown(0))
        {
            dragBeginPosition = MousePosition();
        }
        if (Input.GetMouseButton(0))
        {
            Vector2 min = Vector2.Min(dragBeginPosition, MousePosition());
            Vector2 max = Vector2.Max(dragBeginPosition, MousePosition());
            min = Vector2.Max(min, Vector2.zero);
            max = Vector2.Min(max, new Vector2(Screen.width, Screen.height));
            Vector2 size = max - min;
            selectionRect = new Rect(new Vector2(min.x, min.y), size);
            isDragging = true;
        }
        if (isDragging && selectionRect.size.x > 1 && selectionRect.size.y > 1)
        {
            UniqueIDsFromRT(selectableRT, selectionRect, request =>
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
                Selection.SetHover(ids);
                if (Input.GetMouseButtonUp(0))
                {
                    Selection.ClearHover();
                    Selection.Add(ids);
                }
            });
        }
        if (Input.GetMouseButtonUp(0))
        {
            isDragging = false;
            selectionRect.size = Vector2.zero;
        }
    }

    private void OnGUI()
    {
        if (isDragging)
        {
            Texture2D texture = new Texture2D(1, 1);
            texture.SetPixel(0, 0, new Color(0, 0, 0, 0.15f));
            texture.Apply();
            GUI.DrawTexture(selectionRect, texture);
        }
    }

    private void OnDisable()
    {
        RenderPipelineManager.endCameraRendering -= OnEndCameraRendering;
    }

    public static void VerticallyFlipRenderTexture(RenderTexture renderTexture)
    {
        var temp = RenderTexture.GetTemporary(renderTexture.descriptor);
        Graphics.Blit(renderTexture, temp, new Vector2(1, -1), new Vector2(0, 1));
        Graphics.Blit(temp, renderTexture);
        RenderTexture.ReleaseTemporary(temp);
    }

    private void SampleRenderTextureAtPosition(Vector2 position, RenderTexture renderTexture, Action<AsyncGPUReadbackRequest> callback)
    {
        (int x, int y) = ((int)position.x >> downscaleFactor, (int)position.y >> downscaleFactor);
        AsyncGPUReadback.Request(renderTexture, 0, x, 1, y, 1, 0, 1, callback);
    }

    private bool IsPositionWithinScreen(Vector2 position)
    {
        return position.x >= 0 && ((int)position.x) < (Screen.width) &&
               position.y >= 0 && ((int)position.y) < (Screen.height);
    }
    private Vector2 MousePosition()
    {
        return new Vector2(Input.mousePosition.x, Screen.height - Input.mousePosition.y);
    }

    public void UniqueIDsFromRT(RenderTexture renderTexture, Rect rect, Action<AsyncGPUReadbackRequest> callback)
    {

        outputBuffer = new ComputeBuffer((int)SelectableBase.IDsCount + 1, sizeof(uint));

        computeShader.SetBuffer(initializedKernelID, "Output", outputBuffer);
        computeShader.SetBuffer(mainKernelID, "Output", outputBuffer);
        computeShader.SetTexture(mainKernelID, "Input", renderTexture);
        computeShader.SetVector("Rect", new Vector4(rect.x, rect.y, rect.width, rect.height));
        computeShader.Dispatch(initializedKernelID, Mathf.CeilToInt((int)SelectableBase.IDsCount / 64f), 1, 1);

        var (threadGroupsX, threadGroupsY) = (Mathf.CeilToInt(rect.width / 8f), Mathf.CeilToInt(rect.height / 8f));
        computeShader.Dispatch(mainKernelID, threadGroupsX, threadGroupsY, 1);
        AsyncGPUReadback.Request(outputBuffer, callback);
        outputBuffer.Dispose();
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
