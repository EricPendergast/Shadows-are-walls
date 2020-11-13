using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(SceneLoader), true)]
public class SceneLoaderEditor : Editor {
    public override void OnInspectorGUI() {
        if (GUILayout.Button("Remove Scene")) {
            (target as SceneLoader).EditorRemoveScene();
        }
        if (GUILayout.Button("Open Scene")) {
            (target as SceneLoader).EditorOpenScene();
        }
        base.OnInspectorGUI();
    }

}
