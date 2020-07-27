using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(Positioner), true)]
[CanEditMultipleObjects]
public class PositionerEditor : SnappableObjectEditor {
    public override void OnInspectorGUI() {
        base.OnInspectorGUI();
    }

    public override void OnSceneGUI() {
        if (!Application.isPlaying) {
            var positioner = target as Positioner;

            EditorGUI.BeginChangeCheck();
            var newPos = Handles.PositionHandle(positioner.right, Quaternion.identity);
            if (EditorGUI.EndChangeCheck()) {
                 Undo.RecordObject(positioner, "Change movement path");
                 positioner.right = newPos;
            } 
            positioner.EditorUpdate();
        }

        base.OnSceneGUI();
    }
}
