using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(PolygonCollider2D))]
public abstract class SolidTriangle : LevelObject {
    [SerializeField]
    public Vector2 p1;
    [SerializeField]
    public Vector2 p2;
    [SerializeField]
    public Vector2 p3;
    [SerializeField]
    public bool editing = false;
    private bool firstApplySettings = true;

    protected override void Awake() {
        ApplySettings();
        base.Awake();
    }

    public void ApplySettings() {
        if (firstApplySettings) {
            GetComponent<MeshFilter>().sharedMesh = new Mesh();
        }
        if (TryGetComponent<Snapper>(out var snapper)) {
            p1 = snapper.SnapLocalPoint(p1);
            p2 = snapper.SnapLocalPoint(p2);
            p3 = snapper.SnapLocalPoint(p3);
        }
        var mesh = GetComponent<MeshFilter>().sharedMesh;
        //var mesh = GetComponent<MeshFilter>().sharedMesh;
        //if (mesh == null) {
        //    mesh = GetComponent<MeshFilter>().sharedMesh = new Mesh();
        //}
        mesh.Clear();
        mesh.vertices = new Vector3[]{p1,p2,p3};
        mesh.triangles = new int[]{0,1,2};
        GetComponent<PolygonCollider2D>().points = new Vector2[]{p1, p2, p3};
    }
}
