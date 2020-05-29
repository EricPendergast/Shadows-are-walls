using UnityEngine;

public class Util {
    public static Mesh CreateMeshWithNewMaterial(GameObject go, Material mat) {
        var filter = go.AddComponent<MeshFilter>();
        filter.mesh = new Mesh();
        var renderer = go.AddComponent<MeshRenderer>();
        renderer.materials = new Material[]{new Material(mat)};
        return filter.mesh;
    }

    // This function is a bit problematic because it assumes there is only 1
    // mesh filter on the gameobject
    //public static Mesh GetOrCreateMesh(GameObject go) {
    //    var meshFilter = go.GetComponent<MeshFilter>();
    //    if (meshFilter == null) {
    //        meshFilter = go.AddComponent<MeshFilter>();
    //    }
    //
    //    if (meshFilter.sharedMesh == null) {
    //        meshFilter.sharedMesh = new Mesh();
    //    }
    //    return meshFilter.sharedMesh;
    //}

    public static GameObject CreateChild(Transform parent) {
        var go = new GameObject();
        go.transform.parent = parent;
        return go;
    }

    public static T CreateChild<T>(Transform parent) where T : Component {
        var go = new GameObject();
        go.transform.parent = parent;
        return go.AddComponent<T>();
    }

    public static void SlideDown<T>(ref T? v1, ref T? v2, ref T? v3) where T : struct {
        if (v2 == null) {
            v2 = v3;
            v3 = null;
        }
        if (v1 == null) {
            v1 = v2;
            v2 = v3;
            v3 = null;
        }
    }

    public static void RemoveDuplicates(ref Vector2? v1, ref Vector2? v2, ref Vector2? v3) {
        if (v1 == v2 || v1 == v3) {
            v1 = null;
        }
        if (v2 == v3) {
            v2 = null;
        }
    }
}
