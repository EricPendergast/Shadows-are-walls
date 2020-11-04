using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

// The problem with the existing scene system is that you can't easily tell
// which scenes are loading and unloading. This makes it hard to avoid loading
// duplicate scenes. This class makes any scene's state queryable. (As long as
// it was loaded through the methods in this class).
public class CustomSceneManager {
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void InitAfterSceneLoad() {
        loadingScenes = new HashSet<string>();
        unloadingScenes = new HashSet<string>();
    }

    private static HashSet<string> loadingScenes;
    private static HashSet<string> unloadingScenes;

    public static IEnumerator LoadSceneAsync(SceneReference sceneRef, LoadSceneMode mode) {
        Debug.Assert(IsSceneUnloaded(sceneRef));

        loadingScenes.Add(sceneRef.ScenePath);
        yield return SceneManager.LoadSceneAsync(sceneRef.ScenePath, mode);
        loadingScenes.Remove(sceneRef.ScenePath);
    }

    public static IEnumerator UnloadSceneAsync(SceneReference sceneRef) {
        Debug.Assert(IsSceneLoaded(sceneRef));

        unloadingScenes.Add(sceneRef.ScenePath);
        yield return SceneManager.UnloadSceneAsync(sceneRef.ScenePath);
        unloadingScenes.Remove(sceneRef.ScenePath);
    }

    public static bool IsSceneLoadedOrUnloaded(SceneReference sceneRef) {
        return 
            !loadingScenes.Contains(sceneRef.ScenePath) && 
            !unloadingScenes.Contains(sceneRef.ScenePath);
    }

    public static IEnumerator WaitForSceneLoadedOrUnloaded(SceneReference sceneRef) {
        while (!IsSceneLoadedOrUnloaded(sceneRef)) {
            yield return null;
        }
    }

    public enum SceneState {
        Loading,
        Loaded,
        Unloading,
        Unloaded
    }

    public static SceneState GetSceneState(SceneReference sceneRef) {
        var path = sceneRef.ScenePath;
        if (loadingScenes.Contains(path)) {
            return SceneState.Loading;
        } else if (unloadingScenes.Contains(path)) {
            return SceneState.Unloading;
        } else {
            return SceneManager.GetSceneByPath(path).IsValid() ? SceneState.Loaded : SceneState.Unloaded;
        }
    }

    public static bool IsSceneLoaded(SceneReference sceneRef) {
        return GetSceneState(sceneRef) == SceneState.Loaded;
    }

    public static bool IsSceneUnloaded(SceneReference sceneRef) {
        return GetSceneState(sceneRef) == SceneState.Unloaded;
    }
}
