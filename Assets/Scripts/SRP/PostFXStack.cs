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
        Copy,
        ColorCorrect,
        Grayscale
    }

    List<int> bloomPyramidIds = new List<int>();

    int tmpDestId = Shader.PropertyToID("_tmpDestId");

    public PostFXStack() {
        for (int i = 0; i < 16; i++) {
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

        buffer.GetTemporaryRT(tmpDestId, camera.pixelWidth, camera.pixelHeight);
        RenderTargetIdentifier dest = tmpDestId;

        if (settings.Bloom.enabled) {
            DoBloom(source, dest);
            Swap(ref source, ref dest);
        }

        if (settings.ColorCorrect.enabled) {
            DoColorCorrect(source, dest);
            Swap(ref source, ref dest);
        }

        if (settings.grayscale) {
            DoGrayscale(source, dest);
            Swap(ref source, ref dest);
        }

        Draw(source, BuiltinRenderTextureType.CameraTarget, Pass.Copy);

        context.ExecuteCommandBuffer(buffer);
        buffer.Clear();
    }

    void Swap(ref RenderTargetIdentifier rti1, ref RenderTargetIdentifier rti2) {
        var tmp = rti1;
        rti1 = rti2;
        rti2 = tmp;
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

    void DoGrayscale(RenderTargetIdentifier source, RenderTargetIdentifier dest) {
        Draw(source, dest, Pass.Grayscale);
    }

    void DoColorCorrect(RenderTargetIdentifier source, RenderTargetIdentifier dest) {
        Draw(source, dest, Pass.ColorCorrect);
    }

    void DoBloom(RenderTargetIdentifier source, RenderTargetIdentifier dest) {
        var bloom = settings.Bloom;

        buffer.BeginSample("Bloom");
        int width = camera.pixelWidth/2, height = camera.pixelHeight/2;
        var format = RenderTextureFormat.Default;

        List<int> allocated = new List<int>();

        int i;
        for (i = 0; i < bloom.maxIterations; i++) {

            if (height < bloom.downscaleLimit || width < bloom.downscaleLimit) {
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

        Draw(source, dest, Pass.Copy);
        buffer.EndSample("Bloom");
    }
}
