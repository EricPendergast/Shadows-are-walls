using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Idea: Light finds all opaque objects in
// the scene, calculates all their shadows,
// and then does rendering and collisions
// stuff
[ExecuteInEditMode]
public class FixedLight : MonoBehaviour
{
    public float angle;

    [SerializeField]
    private MeshFilter myMeshFilter;
    private Mesh myMesh;
    [SerializeField]
    private MeshFilter castLightMeshFilter;
    private Mesh castLightMesh;

    void Start() {
        myMeshFilter.sharedMesh = new Mesh();
        myMesh = myMeshFilter.sharedMesh;

        castLightMeshFilter.sharedMesh = new Mesh();
        castLightMesh = castLightMeshFilter.sharedMesh;
    }

    void Update() {
        Vector3 v1 = transform.position;
        Vector3 v2 = transform.position + Quaternion.Euler(0f,0f, angle/2) * gameObject.transform.up;
        Vector3 v3 = transform.position + Quaternion.Euler(0f,0f, -angle/2) * gameObject.transform.up;

        myMesh.vertices = new []{v1, v2, v3};
        myMesh.triangles = new []{0,1,2};
    }
}
