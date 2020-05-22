using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Opaque : MonoBehaviour {
    //public static List<Opaque> instances;
    // Start is called before the first frame update
    void Start() {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void OnDrawGizmos() {
        Gizmos.DrawLine(CrossSection(Vector2.zero).p1, CrossSection(Vector2.zero).p2);
    }

    public LineSegment CrossSection(Vector2 cameraPos) {
        //var boxCollider = GetComponent<BoxCollider2D>();
        var right = transform.right*transform.lossyScale.x;
        var up = transform.up*transform.lossyScale.y;

        var upRight = (right + up)/2;
        var downRight = (right - up)/2;
        return new LineSegment(transform.position - upRight, transform.position + upRight);
    }
}
