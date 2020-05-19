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

    public List<LineSegment> CrossSection(Vector2 cameraPos) {
        var list = new List<LineSegment>();
        var boxCollider = GetComponent<BoxCollider2D>();

        if (boxCollider != null) {
            var upRight = (transform.right + transform.up)/2;
            var downRight = (transform.right - transform.up)/2;
            list.Add(new LineSegment(transform.position - upRight, transform.position + upRight));
            list.Add(new LineSegment(transform.position - downRight, transform.position + downRight));
        }
        
        return list;
    }

    public static List<Opaque> GetAllInstances() {
        return new List<Opaque>(FindObjectsOfType<Opaque>());
    }
}
