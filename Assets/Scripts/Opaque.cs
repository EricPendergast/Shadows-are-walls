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
            list.Add(new LineSegment(transform.position + .5f*Vector3.left, transform.position + .5f*Vector3.right));
        }
        
        return list;
    }

    public static List<Opaque> GetAllInstances() {
        return new List<Opaque>(FindObjectsOfType<Opaque>());
    }
}
