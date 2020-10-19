using UnityEngine;
using UnityEditor;

public class EditorHelper {
    public static void RecordObjectUndo(Object obj, string msg) {
        Undo.RecordObject(obj, msg);
        PrefabUtility.RecordPrefabInstancePropertyModifications(obj);
    }
}
