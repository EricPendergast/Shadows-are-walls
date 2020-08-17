using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
public class LampshadeRenderer : MonoBehaviour {
    [SerializeField]
    private float apertureAngle;
    
    public void OnApertureAngleChange(float apertureAngle) {
        this.apertureAngle = apertureAngle;

        Mesh mesh = GetComponent<MeshFilter>().sharedMesh;
        if (mesh == null) {
            mesh = new Mesh();
            GetComponent<MeshFilter>().sharedMesh = mesh;
        }

        mesh.vertices = new Vector3[]{Vector3.zero, Math.Rotate(Vector3.left, apertureAngle/2), Math.Rotate(Vector3.left, -apertureAngle/2)};
        mesh.triangles = new int[]{0,1,2};
    }
}
