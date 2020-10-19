using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(Positioner), true)]
[CanEditMultipleObjects]
public class PositionerEditor : Editor {
    public override void OnInspectorGUI() {
        base.OnInspectorGUI();
    }

    public void OnSceneGUI() {
        if (!Application.isPlaying) {
            var positioner = target as Positioner;

            EditorGUI.BeginChangeCheck();
            var newDest = Handles.PositionHandle(positioner.EditorGetDestination(), Quaternion.identity);
            if (EditorGUI.EndChangeCheck()) {
                 positioner.EditorSetDestination(newDest);
            } 

            EditorGUI.BeginChangeCheck();
            var origin = positioner.EditorGetOrigin();
            var dest = positioner.EditorGetDestination();
            Vector2 position = positioner.EditorGetPosition();

            Vector2 newPosition = Handles.Slider(position, origin-dest, HandleUtility.GetHandleSize(position)*.25f, Handles.ConeHandleCap, .1f);

            if (EditorGUI.EndChangeCheck()) {
                positioner.EditorSetPosition(newPosition);
            }
        }
    }
}
