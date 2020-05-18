using UnityEngine;

public class Util {
    public static Mesh GetOrCreateMesh(GameObject go) {
        var meshFilter = go.GetComponent<MeshFilter>();
        if (meshFilter == null) {
            meshFilter = go.AddComponent<MeshFilter>();
            meshFilter.mesh = new Mesh();
        }

        return meshFilter.mesh;
    }
}
