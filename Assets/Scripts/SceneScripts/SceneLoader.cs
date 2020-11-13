using UnityEngine;
using System.Collections;

[ExecuteAlways]
public partial class SceneLoader : MonoBehaviour {
    [SerializeField]
    SceneReference sceneToLoad;

    SceneSemaphore sceneSemaphore;
    [SerializeField]
    int requests = 0;

    protected void Awake() {
        if (Application.isPlaying) {
            sceneSemaphore = SceneSemaphore.Create(sceneToLoad);
        }
    }

    partial void EditorUpdate();

    protected virtual void Update() {
        if (!Application.isPlaying) {
            EditorUpdate();
        }
    }

    public void RequestLoad() {
        requests++;
        Debug.Assert(Application.isPlaying);
        StartCoroutine(sceneSemaphore.RequestLoad());
    }

    public void RequestUnload() {
        requests--;
        Debug.Assert(Application.isPlaying);
        StartCoroutine(sceneSemaphore.RequestUnload());
    }

    public SceneReference GetSceneToLoad() {
        return sceneToLoad;
    }
}
