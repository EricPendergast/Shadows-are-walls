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

        var upRight = (transform.right + transform.up)/2;
        var downRight = (transform.right - transform.up)/2;
        return new LineSegment(transform.position - upRight, transform.position + upRight);
    }

    public static List<Opaque> GetAllInstances() {
        return new List<Opaque>(FindObjectsOfType<Opaque>());
    }

    public void CreateShadow(LightBase light) {
        // TODO: There are better ways to handle this
        var shadowGameObject = new GameObject();
        shadowGameObject.transform.SetParent(transform);
        var shadow = shadowGameObject.AddComponent<Shadow>();
        shadow.Init(this, light);
    }

    public void DestroyShadow(LightBase light) {
        foreach (Transform child in transform) {
            Debug.Log("Trying to destroy");
            if (child.TryGetComponent(out Shadow shadow) && shadow.sourceLight == light) {
                Destroy(child.gameObject);
            }
        }
    }
}
