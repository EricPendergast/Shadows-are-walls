using UnityEngine;

public class CameraEffects : MonoBehaviour {
    [SerializeField]
    private Material blurMaterial;
    [SerializeField]
    private int blurWidth;
    [SerializeField]
    private int blurHeight;
    [SerializeField]
    private float blurScale;

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
        InitRenderTexture(ref blur1, (int)(source.width*blurScale), (int)(source.height*blurScale));
        InitRenderTexture(ref blur2, (int)(source.width*blurScale), (int)(source.height*blurScale));
        InitRenderTexture(ref colorCorrectTex, source.width, source.height);

        Graphics.Blit(source, blur1, isolateBrightMaterial);

        SetBlurParams(source, blurWidth, 0);
        Graphics.Blit(blur1, blur2, blurMaterial);
        
        SetBlurParams(source, 0, blurHeight);
        Graphics.Blit(blur2, blur1, blurMaterial);
        
        SetBloomBlendParams(blur1);
        Graphics.Blit(source, colorCorrectTex, bloomBlendMaterial);
        
        Graphics.Blit(colorCorrectTex, destination, colorCorrectMaterial);
    }

    void InitRenderTexture(ref RenderTexture tex, int width, int height) {
        if (tex == null) {
            tex = RenderTexture.GetTemporary(width, height, 0, RenderTextureFormat.DefaultHDR);
        }
    }

    void SetBlurParams(RenderTexture source, int blurWidth, int blurHeight) {
        blurMaterial.SetFloat("_texelWidth", 1.0f/source.width);
        blurMaterial.SetFloat("_texelHeight", 1.0f/source.height);
        blurMaterial.SetInt("_width", blurWidth);
        blurMaterial.SetInt("_height", blurHeight);
    }

    void SetBloomBlendParams(RenderTexture lightTex) {
        bloomBlendMaterial.SetTexture("_lightTex", lightTex);
    }

    void OnDestroy() {
        RenderTexture.ReleaseTemporary(blur1);
        RenderTexture.ReleaseTemporary(blur2);
    }
}
