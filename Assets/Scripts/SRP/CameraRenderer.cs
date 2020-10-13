using UnityEngine;
using UnityEngine.Rendering;

public partial class CameraRenderer {
	ScriptableRenderContext context;
	Camera camera;
    const string bufferName = "Render Camera";
    static ShaderTagId unlitShaderTagId = new ShaderTagId("SRPDefaultUnlit");
    static int frameBufferId = Shader.PropertyToID("_CameraFrameBuffer");

    CommandBuffer buffer = new CommandBuffer { name = bufferName };

    CullingResults cullingResults;

    PostFXStack postFXStack = new PostFXStack();

	public void Render (ScriptableRenderContext context, Camera camera, PostFXSettings postFXSettings) {
		this.context = context;
		this.camera = camera;

        PrepareForSceneWindow();

        if (!Cull()) {
            return;
        }

        postFXStack.Setup(context, camera, postFXSettings);
        Setup();
        DrawVisibleGeometry();
        DrawGizmosBeforeFX();
        if (postFXStack.IsActive) {
            postFXStack.Render(frameBufferId);
        }
        DrawGizmosAfterFX();
        Cleanup();
        Submit();
	}

    private bool Cull() {
        if (camera.TryGetCullingParameters(out var cullingParameters)) {
            cullingResults = context.Cull(ref cullingParameters);
            return true;
        }
        return false;
    }

    private void Setup() {
        context.SetupCameraProperties(camera);
        var flags = camera.clearFlags;

        if (postFXStack.IsActive) {
            if (flags > CameraClearFlags.Color) {
                flags = CameraClearFlags.Color;
            }
            buffer.GetTemporaryRT(
                frameBufferId, camera.pixelWidth, camera.pixelHeight,
                32, FilterMode.Bilinear, RenderTextureFormat.Default
            );

            buffer.SetRenderTarget(
                frameBufferId,
                RenderBufferLoadAction.DontCare, RenderBufferStoreAction.Store
            );
        }

        buffer.ClearRenderTarget(
            flags != CameraClearFlags.Nothing,
            flags != CameraClearFlags.Nothing && flags != CameraClearFlags.Depth,
            camera.backgroundColor.linear);

        buffer.BeginSample(bufferName);
        ExecuteBuffer();
    }

    private void DrawVisibleGeometry() {
        context.DrawSkybox(camera);
        
        // Drawing opaque objects
        var sortingSettings = new SortingSettings(camera);
        var drawingSettings = new DrawingSettings(unlitShaderTagId, sortingSettings);
        var filteringSettings = new FilteringSettings(RenderQueueRange.opaque);
        
        context.DrawRenderers(
            cullingResults, ref drawingSettings, ref filteringSettings
        );

        // Drawing transparent objects
        sortingSettings.criteria = SortingCriteria.CommonTransparent;
        drawingSettings.sortingSettings = sortingSettings;
        filteringSettings.renderQueueRange = RenderQueueRange.transparent;
        context.DrawRenderers(
            cullingResults, ref drawingSettings, ref filteringSettings
        );
    }

    private void Submit() {
        buffer.EndSample(bufferName);
        ExecuteBuffer();
        context.Submit();
    }

    private void ExecuteBuffer() {
        context.ExecuteCommandBuffer(buffer);
        buffer.Clear();
    }

    private void Cleanup() {
        if (postFXStack.IsActive) {
            buffer.ReleaseTemporaryRT(frameBufferId);
        }
    }
}
