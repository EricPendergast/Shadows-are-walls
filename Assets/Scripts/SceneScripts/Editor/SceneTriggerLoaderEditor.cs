using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(SceneTriggerLoader), true)]
public class SceneTriggerLoaderEditor : Editor {
    public override void OnInspectorGUI() {
        if (GUILayout.Button("Remove Scene")) {
            (target as SceneTriggerLoader).EditorRemoveScene();
        }
        if (GUILayout.Button("Open Scene")) {
            (target as SceneTriggerLoader).EditorOpenScene();
        }
        base.OnInspectorGUI();
    }

}
