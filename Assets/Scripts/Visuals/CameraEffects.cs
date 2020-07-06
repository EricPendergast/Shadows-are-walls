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

    [SerializeField]
    private Material sketchyEffectMaterial;

    private Camera lightCamera;

    //void Awake() {
    //    lightCamera = Util.CreateChild<Camera>(transform);
    //    lightCamera.enabled = false;
    //}

    void ApplyMaterial(ref RenderTexture source, Material mat) {
        RenderTexture dest = RenderTexture.GetTemporary(source.descriptor);
        Graphics.Blit(source, dest, mat);
        RenderTexture.ReleaseTemporary(source);
        source = dest;
    }

    void ApplyBlur(ref RenderTexture source, Material blurMat, bool horizontal) {
        SetBlurParams(ref blurMat, source, horizontal);
        ApplyMaterial(ref source, blurMat);
    }

    void Clear(RenderTexture tex) {
        RenderTexture rt = UnityEngine.RenderTexture.active;
        UnityEngine.RenderTexture.active = tex;
        GL.Clear(true, true, Color.clear);
        UnityEngine.RenderTexture.active = rt;
    }

    void OnRenderImage(RenderTexture source, RenderTexture destination) {

        var bloomTexture = RenderTexture.GetTemporary(source.descriptor);
        Graphics.Blit(source, bloomTexture, isolateBrightMaterial);

        ApplyBlur(ref bloomTexture, blurPass1, true);
        ApplyBlur(ref bloomTexture, blurPass1, false);

        ApplyBlur(ref bloomTexture, blurPass2, true);
        ApplyBlur(ref bloomTexture, blurPass2, false);
        //lightCamera.CopyFrom(GetComponent<Camera>());
        //lightCamera.targetTexture = blur2;
        //lightCamera.cullingMask = PhysicsHelper.lightLayerMask;
        //lightCamera.Render();
        //Graphics.Blit(blur2, blur1, isolateBrightMaterial);
        
        SetSketchyEffectParams(bloomTexture);
        ApplyMaterial(ref source, sketchyEffectMaterial);
        SetBloomBlendParams(null);
        RenderTexture.ReleaseTemporary(bloomTexture);
        
        Graphics.Blit(source, destination, colorCorrectMaterial);
        RenderTexture.ReleaseTemporary(source);
    }

    void SetBloomBlendParams(RenderTexture lightTex) {
        bloomBlendMaterial.SetTexture("_lightTex", lightTex);
    }

    void SetSketchyEffectParams(RenderTexture lightTex) {
        sketchyEffectMaterial.SetTexture("_lightTex", lightTex);
        //sketchyEffectMaterial.SetMatrix("cameraToWorld", GetComponent<Camera>().cameraToWorldMatrix.inverse);
    }

    public void SetBlurParams(ref Material blurMaterial, RenderTexture source, bool horizontal) {
        blurMaterial.SetFloat("_texelWidth", 1.0f/source.width);
        blurMaterial.SetFloat("_texelHeight", 1.0f/source.height);
        blurMaterial.SetFloat("_texelsPerUnit", source.height/(2*GetComponent<Camera>().orthographicSize));
        blurMaterial.SetInt("_horizontal", horizontal ? 1 : 0);
    }
}
