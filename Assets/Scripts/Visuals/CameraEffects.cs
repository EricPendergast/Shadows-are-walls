using UnityEngine;

public class CameraEffects : MonoBehaviour {
    [SerializeField]
    private Material blurPass1;

    [SerializeField]
    private Material blurPass2;

    [SerializeField]
    private Material isolateBrightMaterial;

    [SerializeField]
    private Material bloomBlendMaterial;

    [SerializeField]
    private Material colorCorrectMaterial;

    private RenderTexture blur1;
    private RenderTexture blur2;
    private RenderTexture colorCorrectTex;

    void OnRenderImage(RenderTexture source, RenderTexture destination) {
        int scale = 4;
        InitRenderTexture(ref blur1, source.width/scale, source.height/scale);
        InitRenderTexture(ref blur2, source.width/scale, source.height/scale);
        InitRenderTexture(ref colorCorrectTex, source.width, source.height);

        Graphics.Blit(source, blur1, isolateBrightMaterial);

        SetBlurParams(blurPass1, source, true);
        Graphics.Blit(blur1, blur2, blurPass1);
        SetBlurParams(blurPass1, source, false);
        Graphics.Blit(blur2, blur1, blurPass1);

        SetBlurParams(blurPass2, source, true);
        Graphics.Blit(blur1, blur2, blurPass2);
        SetBlurParams(blurPass2, source, false);
        Graphics.Blit(blur2, blur1, blurPass2);
        
        SetBloomBlendParams(blur1);
        Graphics.Blit(source, colorCorrectTex, bloomBlendMaterial);
        
        Graphics.Blit(colorCorrectTex, destination, colorCorrectMaterial);
    }

    void InitRenderTexture(ref RenderTexture tex, int width, int height) {
        if (tex == null) {
            tex = RenderTexture.GetTemporary(width, height, 0, RenderTextureFormat.DefaultHDR);
        }
    }

    void SetBlurParams(Material blurMaterial, RenderTexture source, bool horizontal) {
        blurMaterial.SetFloat("_texelWidth", 1.0f/source.width);
        blurMaterial.SetFloat("_texelHeight", 1.0f/source.height);
        blurMaterial.SetInt("_horizontal", horizontal ? 1 : 0);
    }

    void SetBloomBlendParams(RenderTexture lightTex) {
        bloomBlendMaterial.SetTexture("_lightTex", lightTex);
    }

    void OnDestroy() {
        RenderTexture.ReleaseTemporary(blur1);
        RenderTexture.ReleaseTemporary(blur2);
    }
}
