using UnityEngine.Rendering;
using UnityEngine;

public class CustomPipeline : RenderPipeline {
    CameraRenderer cameraRenderer = new CameraRenderer();
    PostFXSettings postFXSettings;

    public CustomPipeline(PostFXSettings postFXSettings) {
        this.postFXSettings = postFXSettings;
    }

    protected override void Render(ScriptableRenderContext renderContext, Camera[] cameras) {
        foreach (var camera in cameras) {
            cameraRenderer.Render(renderContext, camera, postFXSettings);
        }
    }
}
