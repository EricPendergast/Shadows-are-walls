using UnityEngine;

static class Extension {
    public static void SetBlurParams(this Material blurMaterial, RenderTexture source, bool horizontal) {
        blurMaterial.SetFloat("_texelWidth", 1.0f/source.width);
        blurMaterial.SetFloat("_texelHeight", 1.0f/source.height);
        blurMaterial.SetInt("_horizontal", horizontal ? 1 : 0);
    }
}


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

    private Camera lightCamera;

    void Awake() {
        lightCamera = Util.CreateChild<Camera>(transform);
        lightCamera.enabled = false;
    }

    void OnRenderImage(RenderTexture source, RenderTexture destination) {
        int scale = 4;
        InitTexture(ref blur1, source.width/scale, source.height/scale);
        InitTexture(ref blur2, source.width/scale, source.height/scale);
        InitTexture(ref colorCorrectTex, source.width, source.height);

        lightCamera.CopyFrom(GetComponent<Camera>());
        lightCamera.targetTexture = blur2;
        lightCamera.cullingMask = PhysicsHelper.lightLayerMask;
        lightCamera.Render();

        Graphics.Blit(blur2, blur1, isolateBrightMaterial);

        blurPass1.SetBlurParams(source, true);
        Graphics.Blit(blur1, blur2, blurPass1);
        blurPass1.SetBlurParams(source, false);
        Graphics.Blit(blur2, blur1, blurPass1);

        blurPass2.SetBlurParams(source, true);
        Graphics.Blit(blur1, blur2, blurPass2);
        blurPass2.SetBlurParams(source, false);
        Graphics.Blit(blur2, blur1, blurPass2);
        
        SetBloomBlendParams(blur1);
        Graphics.Blit(source, colorCorrectTex, bloomBlendMaterial);
        
        Graphics.Blit(colorCorrectTex, destination, colorCorrectMaterial);
    }

    void SetBloomBlendParams(RenderTexture lightTex) {
        bloomBlendMaterial.SetTexture("_lightTex", lightTex);
    }

    void OnDestroy() {
        RenderTexture.ReleaseTemporary(blur1);
        RenderTexture.ReleaseTemporary(blur2);
    }

    public static void InitTexture(ref RenderTexture tex, int width, int height) {
        if (tex == null || tex.width != width || tex.height != height) {
            tex = RenderTexture.GetTemporary(width, height, 0, RenderTextureFormat.DefaultHDR);
        }
    }

}
