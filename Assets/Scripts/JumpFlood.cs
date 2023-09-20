using System;
using UnityEngine;
using UnityEngine.Rendering;

[Serializable]
public class JumpFlood
{
    public const int PASS_INIT = 0;
    public const int PASS_JUMP = 1;

    [field: SerializeField, Range(0, 32)]
    public int Steps { get; set; }

    private Material material;

    private RenderTexture GetTemp(RenderTexture sourceTex)
    {
        var tex = RenderTexture.GetTemporary(sourceTex.width,
                                             sourceTex.height,
                                             0,
                                             RenderTextureFormat.ARGBFloat,
                                             RenderTextureReadWrite.Linear, 1);
        tex.wrapMode = TextureWrapMode.Clamp;
        return tex;
    }

    private bool tryInitMaterial()
    {
        if (material != null)
        {
            return true;
        }

        var shader = Shader.Find("Hidden/ScreenSpaceDistanceField");
        if (shader == null)
        {
            return false;
        }

        material = new Material(shader);
        material.hideFlags = HideFlags.HideAndDontSave;
        return true;
    }

    /// <summary>
    /// Given a render texture, returns a render texture of the same dimensions.
    /// The returned render texture has the type ARGBFloat.  The components have
    /// the following meanings:
    ///   X: x distance to closest surface point
    ///   Y: y distance to closest surface point
    ///   Z: squared distance to closest surface point
    ///   W: zero if outside the surface, nonzero if inside
    ///   
    /// The units of X Y and Z are all normalized to the HEIGHT of the texture.
    /// If Z is a distance of 1, that means that the closest surface is HEIGHT
    /// pixels away.
    /// 
    /// The surface of the input texture is determined by thresholding the alpha
    /// component against 0.5.  Values less than 0.5 are considered outside, and
    /// values greater than 0.5 are considered inside.
    /// 
    /// The texture returned is a TEMPORARY texture, and must be released once
    /// you are finished with it.
    /// </summary>
    public RenderTexture BuildDistanceField(RenderTexture sourceTex)
    {
        if (!tryInitMaterial())
        {
            return null;
        }

        Steps = Mathf.Clamp(Steps, 0, 32);

        var tex0 = GetTemp(sourceTex);
        var tex1 = GetTemp(sourceTex);

        Graphics.Blit(sourceTex, tex0, material, PASS_INIT);

        int step = Mathf.RoundToInt(Mathf.Pow(Steps - 1, 2));
        while (step != 0)
        {
            material.SetFloat("_Step", step);
            Graphics.Blit(tex0, tex1, material, PASS_JUMP);

            RenderTexture tmp = tex0;
            tex0 = tex1;
            tex1 = tmp;

            step /= 2;
        }

        RenderTexture.ReleaseTemporary(tex1);
        return tex0;
    }
}