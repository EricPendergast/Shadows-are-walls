using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneSemaphore {
    private int numLoadRequests = 0;
    SceneReference sceneToLoad;

    private static Dictionary<string, SceneSemaphore> allSemaphores;
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void StaticInit() {
        allSemaphores = new Dictionary<string, SceneSemaphore>();
    }

    public static SceneSemaphore Create(SceneReference sceneToLoad) {
        if (allSemaphores.ContainsKey(sceneToLoad.ScenePath)) {
            return allSemaphores[sceneToLoad.ScenePath];
        } else {
            var sceneSemaphore = new SceneSemaphore(sceneToLoad);
            allSemaphores.Add(sceneToLoad.ScenePath, sceneSemaphore);
            return sceneSemaphore;
        }
    }

    private SceneSemaphore(SceneReference sceneToLoad) {
        this.sceneToLoad = sceneToLoad;
    }

    public IEnumerator RequestLoad() {
        numLoadRequests++;
        yield return CustomSceneManager.WaitForSceneLoadedOrUnloaded(sceneToLoad);
        if (numLoadRequests > 0 && CustomSceneManager.IsSceneUnloaded(sceneToLoad)) {
            yield return CustomSceneManager.LoadSceneAsync(sceneToLoad, LoadSceneMode.Additive);
        }
    }

    public IEnumerator RequestUnload() {
        numLoadRequests--;
        yield return CustomSceneManager.WaitForSceneLoadedOrUnloaded(sceneToLoad);
        if (numLoadRequests < 1 && CustomSceneManager.IsSceneLoaded(sceneToLoad)) {
            yield return CustomSceneManager.UnloadSceneAsync(sceneToLoad);
        }
    }
}
