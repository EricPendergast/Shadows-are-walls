using UnityEngine;
using System.Collections.Generic;

[ExecuteAlways]
public partial class SceneTriggerLoader : MonoBehaviour {
    [SerializeField]
    List<SceneTriggerLoader> adjacent;
    [SerializeField]
    bool loadAdjacent = true;

    private SceneLoader sceneLoader;
    
    void Awake() {
        sceneLoader = GetComponent<SceneLoader>();
    }

    partial void EditorUpdate();

    void Update() {
        if(!Application.isPlaying) {
            EditorUpdate();
        }
    }
    
    void OnTriggerEnter2D(Collider2D collider) {
        if (Application.isPlaying) {
            if (collider.TryGetComponent(out Player.Player _)) {
                RequestLoad();
                if (loadAdjacent) {
                    foreach (var loader in adjacent) {
                        loader.RequestLoad();
                    }
                }
            }
        }
    }

    void OnTriggerExit2D(Collider2D collider) {
        if (Application.isPlaying) {
            if (collider.TryGetComponent(out Player.Player _)) {
                RequestUnload();
                if (loadAdjacent) {
                    foreach (var loader in adjacent) {
                        loader.RequestUnload();
                    }
                }
            }
        }
    }

    void RequestUnload() {
        sceneLoader.RequestUnload();
    }

    void RequestLoad() {
        sceneLoader.RequestLoad();
    }
}
