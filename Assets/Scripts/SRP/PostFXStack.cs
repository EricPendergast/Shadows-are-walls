using UnityEngine;
using UnityEngine.Rendering;
using System.Collections.Generic;

public partial class PostFXStack {
    const string bufferName = "Post FX";

    CommandBuffer buffer = new CommandBuffer {
        name = bufferName
    };

    ScriptableRenderContext context;

    Camera camera;

    PostFXSettings settings;

    int fxSourceId = Shader.PropertyToID("_PostFXSource");

    enum Pass {
        Copy
    }

    List<int> bloomPyramidIds = new List<int>();

    public PostFXStack() {
        for (int i = 0; i < 5; i++) {
            bloomPyramidIds.Add(Shader.PropertyToID("_BloomPyramid" + i));
        }
    }

    public void Setup(ScriptableRenderContext context, Camera camera, PostFXSettings settings) {
        this.context = context;
        this.camera = camera;
        this.settings = camera.cameraType <= CameraType.SceneView ? settings : null;

        ApplySceneViewState();
    }

    public bool IsActive => settings != null;

    public void Render(RenderTargetIdentifier source) {
        //Draw(source, BuiltinRenderTextureType.CameraTarget, Pass.Copy);
        DoBloom(source);
        context.ExecuteCommandBuffer(buffer);
        buffer.Clear();
    }

    void Draw(RenderTargetIdentifier source, RenderTargetIdentifier dest, Pass pass) {
        buffer.SetGlobalTexture(fxSourceId, source);
        buffer.SetRenderTarget(
            dest, RenderBufferLoadAction.DontCare, RenderBufferStoreAction.Store
        );

        buffer.DrawProcedural(
            Matrix4x4.identity, settings.Material, (int)pass,
            MeshTopology.Triangles, 3
        );
    }

    void DoBloom(RenderTargetIdentifier source) {
        buffer.BeginSample("Bloom");
        int width = camera.pixelWidth/2, height = camera.pixelHeight/2;
        var format = RenderTextureFormat.Default;

        List<int> allocated = new List<int>();

        int i;
        for (i = 0; i < bloomPyramidIds.Count; i++) {

            if (height < 1 || width < 1) {
                break;
            }
            int destId = bloomPyramidIds[i];
            buffer.GetTemporaryRT(
                destId, width, height, 0, FilterMode.Bilinear, format
            );
            allocated.Add(destId);

            Draw(source, destId, Pass.Copy);

            source = destId;
            width /= 2;
            height /= 2;
        }

        foreach (var renderTextureId in allocated) {
            buffer.ReleaseTemporaryRT(renderTextureId);
        }

        Draw(source, BuiltinRenderTextureType.CameraTarget, Pass.Copy);


        buffer.EndSample("Bloom");
    }
}
