using UnityEngine;

//[RequireComponent(typeof(Rigidbody2D))]
public interface Opaque {
    //public Vector2 p1;
    //public Vector2 p2;

    //void Awake() {
    //    gameObject.layer = LayerMask.NameToLayer("Opaque");
    //}
    //
    //void OnDrawGizmos() {
    //    Gizmos.DrawLine(CrossSection(Vector2.zero).p1, CrossSection(Vector2.zero).p2);
    //}

    LineSegment CrossSection(Vector2 cameraPos);
        //return new LineSegment(transform.TransformPoint(p1), transform.TransformPoint(p2));
}
