using UnityEngine;

public class Util {
    public static Mesh GetOrCreateMesh(GameObject go) {
        var meshFilter = go.GetComponent<MeshFilter>();
        if (meshFilter == null) {
            meshFilter = go.AddComponent<MeshFilter>();
        }

        if (meshFilter.mesh == null) {
            meshFilter.mesh = new Mesh();
        }
        return meshFilter.mesh;
    }

    public static T CreateChild<T>(Transform parent) where T : Component {
        var go = new GameObject();
        go.transform.parent = parent;
        return go.AddComponent<T>();
    }
}
