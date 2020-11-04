using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadSceneOnStartup : MonoBehaviour {
    public SceneReference toLoad;

    IEnumerator Start() {
        yield return CustomSceneManager.WaitForSceneLoadedOrUnloaded(toLoad);
        if (CustomSceneManager.IsSceneUnloaded(toLoad)) {
            yield return CustomSceneManager.LoadSceneAsync(toLoad, LoadSceneMode.Additive);
        }
    }
}
