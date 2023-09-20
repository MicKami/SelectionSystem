using System;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

public class SelectionOutlineRenderer : MonoBehaviour
{
    public float outlineWidth = 0.0035f;
    public Color outlineColor = Color.cyan;
    public Material selectionUnlit;
    public Material composite;
    public JumpFlood jumpFlood;

    private void OnEnable()
    {
        RenderPipelineManager.endCameraRendering += OnEndCameraRendering;
    }

    private void OnEndCameraRendering(ScriptableRenderContext context, Camera camera)
    {
        if (camera.cameraType != CameraType.Game) return;
        if (Selection.Active.Count > 0)
        {
            CommandBuffer cmd = CommandBufferPool.Get("SelectionOutline");
            var selectionRT = RenderTexture.GetTemporary(Screen.width, Screen.height, 0, RenderTextureFormat.ARGBFloat, RenderTextureReadWrite.Linear, QualitySettings.antiAliasing);
            cmd.SetRenderTarget(selectionRT);
            //cmd.ClearRenderTarget(false, true, Color.clear);
            foreach (var item in Selection.Active)
            {
                cmd.DrawRenderer(item.Renderer, selectionUnlit, 0, 0);
            }
            context.ExecuteCommandBuffer(cmd);
            context.Submit();

            cmd.Clear();

            var JFA = jumpFlood.BuildDistanceField(selectionRT);
            Shader.SetGlobalTexture("_TargetTexture", selectionRT);
            composite.SetFloat("_OutlineWidth", outlineWidth);
            composite.SetColor("_OutlineColor", outlineColor);

            cmd.Blit(JFA, BuiltinRenderTextureType.CameraTarget, composite);
            RenderTexture.ReleaseTemporary(selectionRT);
            RenderTexture.ReleaseTemporary(JFA);
            context.ExecuteCommandBuffer(cmd);
            context.Submit();
            CommandBufferPool.Release(cmd);
        }

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

    }
    private void OnDisable()
    {
        RenderPipelineManager.endCameraRendering -= OnEndCameraRendering;
    }   
}
