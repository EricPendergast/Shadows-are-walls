using UnityEngine;
using System.Collections;

[ExecuteAlways]
public partial class SceneTriggerLoader : MonoBehaviour {
    [SerializeField]
    SceneReference sceneToLoad;

    SceneSemaphore sceneSemaphore;

    void Awake() {
        if (Application.isPlaying) {
            sceneSemaphore = SceneSemaphore.Create(sceneToLoad);
        }
    }

    partial void EditorUpdate();

    void Update() {
        if (!Application.isPlaying) {
            EditorUpdate();
        }
    }

    IEnumerator OnTriggerEnter2D(Collider2D collider) {
        if (Application.isPlaying) {
            if (collider.TryGetComponent(out Player.Player _)) {
                yield return sceneSemaphore.RequestLoad();
            }
        }
    }

    IEnumerator OnTriggerExit2D(Collider2D collider) {
        if (Application.isPlaying) {
            if (collider.TryGetComponent(out Player.Player _)) {
                yield return sceneSemaphore.RequestUnload();
            }
        }
    }
}
