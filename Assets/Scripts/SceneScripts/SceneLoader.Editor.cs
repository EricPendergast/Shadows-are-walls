#if UNITY_EDITOR
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using UnityEngine;

public partial class SceneLoader {
    public void EditorOpenScene() {
        EditorSceneManager.OpenScene(sceneToLoad.ScenePath, OpenSceneMode.Additive);
    }

    public void EditorRemoveScene() {
        Scene toRemove = SceneManager.GetSceneByPath(sceneToLoad.ScenePath);
        bool saved = EditorSceneManager.SaveModifiedScenesIfUserWantsTo(new Scene[]{toRemove});
        if (saved) {
            EditorSceneManager.CloseScene(toRemove, true);
        }
    }

    partial void EditorUpdate() {
        if (sceneToLoad == null) {
            return;
        }

        EditorHelper.RecordObjectUndo(gameObject, "Rename game object");
        if (sceneToLoad.ScenePath != "") {
            var path = sceneToLoad.ScenePath;
            int start = path.LastIndexOf('/') + 1;
            gameObject.name = path.Substring(start, path.LastIndexOf('.') - start);
        }

        Scene currentScene = EditorSceneManager.GetSceneByPath(sceneToLoad.ScenePath);
        if (currentScene.IsValid() && currentScene.isLoaded) {
            if (currentScene.rootCount == 1) {
                var root = currentScene.GetRootGameObjects()[0];
                EditorHelper.RecordObjectUndo(root.transform, "Move scene root");

                if (TryGetComponent(out Snapper snapper)) {
                    snapper.DoSnapping();
                }

                EditorHelper.RecordObjectUndo(root.transform, "Snap transform");
                root.transform.position = transform.position;
            } else {
                Debug.LogError("Error: Scene has more than one root object. It will not be repositioned.");
            }
        }
    }
}

#endif
