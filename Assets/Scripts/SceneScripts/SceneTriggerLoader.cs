using UnityEngine;
using System.Collections;

public partial class SceneTriggerLoader : MonoBehaviour {
    [SerializeField]
    SceneReference sceneToLoad;

    SceneSemaphore sceneSemaphore;

    void Awake() {
        sceneSemaphore = SceneSemaphore.Create(sceneToLoad);
    }

    IEnumerator OnTriggerEnter2D(Collider2D collider) {
        if (collider.tag == "Player") {
            yield return sceneSemaphore.RequestLoad();
        }
    }

    IEnumerator OnTriggerExit2D(Collider2D collider) {
        if (collider.tag == "Player") {
            yield return sceneSemaphore.RequestUnload();
        }
    }
}
