using System.Collections.Generic;
using System.Collections;
using UnityEngine;

public class Util {
    public static Mesh CreateMeshWithSharedMaterial(GameObject go, Material mat) {
        var filter = go.AddComponent<MeshFilter>();
        filter.mesh = new Mesh();
        var renderer = go.AddComponent<MeshRenderer>();
        renderer.sharedMaterials = new Material[]{mat};
        return filter.mesh;
    }

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
        go.transform.SetParent(parent, false);
        return go;
    }

    public static T CreateChild<T>(Transform parent) where T : Component {
        var go = new GameObject();
        go.transform.SetParent(parent, false);
        return go.AddComponent<T>();
    }

    public static void SlideDown<T>(ref T? v1, ref T? v2) where T : struct {
        if (v1 == null) {
            v1 = v2;
            v2 = null;
        }
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

    public static void RemoveDuplicates(ref Vector2? v1, ref Vector2? v2, ref Vector2? v3, float epsilon) {
        bool v1EqV2 = Math.ApproxEq(v1, v2, epsilon);
        bool v1EqV3 = Math.ApproxEq(v1, v3, epsilon);
        if (v1EqV2 || v1EqV3) {
            v1 = null;
            // This handles transitivity. Specifically, if the points are lined
            // up in a row, each spaced epsilon apart, they should all be
            // considered the same point, even though the ends are 2*epsilon
            // apart
            if (v1EqV2 && v1EqV3) {
                v2 = null;
            }
        }
        if (Math.ApproxEq(v2, v3, epsilon)) {
            v2 = null;
        }
    }

    public static Vector2 Round(Vector2 vec) {
        return new Vector2(
                Mathf.Round(vec.x),
                Mathf.Round(vec.y));
    }

    public static Vector3 Round(Vector3 vec) {
        return new Vector3(
                Mathf.Round(vec.x),
                Mathf.Round(vec.y),
                Mathf.Round(vec.z));
    }

    public static IEnumerable<Component> AllChildrenComponentsIter(GameObject gameObject) {
        foreach (GameObject child in AllChildrenIter(gameObject)) {
            foreach (Component component in child.GetComponents<Component>()) {
                yield return component;
            }
        }
    }

    public static IEnumerable<GameObject> AllChildrenIter(GameObject gameObject) {
        foreach (Transform child in gameObject.transform) {
            yield return child.gameObject;
            foreach (GameObject subChild in AllChildrenIter(child.gameObject)) {
                yield return subChild;
            }
        }
    }
}
