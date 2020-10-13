using UnityEngine;
using UnityEngine.Rendering;

[CreateAssetMenu(menuName = "Rendering/Custom Pipeline")]
public class CustomPipelineAsset : RenderPipelineAsset {

    [SerializeField]
    PostFXSettings postFXSettings = default;

    protected override RenderPipeline CreatePipeline() {
        return new CustomPipeline(postFXSettings);
    }
}
