using UnityEngine;

[CreateAssetMenu(menuName = "Rendering/Custom Post FX Settings")]
public class PostFXSettings : ScriptableObject {
    [SerializeField]
    Shader shader = default;

    [System.NonSerialized]
    Material material;

    [System.Serializable]
    public struct BloomSettings {
        public bool enabled;
        [Range(0f, 16f)]
        public int maxIterations;
        [Min(1f)]
        public int downscaleLimit;
    }

    [SerializeField]
    BloomSettings bloom = default;
    public BloomSettings Bloom => bloom;

    [System.Serializable]
    public struct ColorCorrectSettings {
        public bool enabled;
    }

    [SerializeField]
    ColorCorrectSettings colorCorrect = default;
    public ColorCorrectSettings ColorCorrect => colorCorrect;

    [SerializeField]
    public bool grayscale;

    public Material Material {
        get {
            if (material == null && shader != null) {
                material = new Material(shader);
                material.hideFlags = HideFlags.HideAndDontSave;
            }
            return material;
        }
    }
}
