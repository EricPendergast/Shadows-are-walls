using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(Snapper), true)]
[CanEditMultipleObjects]
public class SnapperEditor : Editor {
    public override void OnInspectorGUI() {
        base.OnInspectorGUI();
    }

    public void OnSceneGUI() {
        if (!Application.isPlaying) {
            Undo.RecordObject((target as Snapper).transform, "Snap");
            PrefabUtility.RecordPrefabInstancePropertyModifications((target as Snapper).transform);
            (target as Snapper).DoSnapping();
        }
    }
}

