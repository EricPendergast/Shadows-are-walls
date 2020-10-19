using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class EditorHelper {
    public static void RecordObjectUndo(Object obj, string msg) {
#if UNITY_EDITOR
        Undo.RecordObject(obj, msg);
        PrefabUtility.RecordPrefabInstancePropertyModifications(obj);
#endif
    }
}
