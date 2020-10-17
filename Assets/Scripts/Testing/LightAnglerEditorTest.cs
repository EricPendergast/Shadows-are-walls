using UnityEngine;

// This class just isolates an editor bug. To reproduce, put this script on a
// game object, and click and drag to change the value of "currentAngle". If
// the bug is still around, it will not slide smoothly.
[RequireComponent(typeof(Rigidbody2D))]
public class LightAnglerEditorTest : MonoBehaviour {
    
    public float u1 = 1.0f;
    
    public float u2 = 1.0f;
    
    public float u3 = 1.0f;
    
    public float u4 = 1.0f;
    
    public float u5 = 1.0f;
    
    public float u6 = 1.0f;
    
    public float u7 = 1.0f;
    
    public float u8 = 1.0f;
    
    public float u9 = 1.0f;
    
    public float u10 = 1.0f;
    
    public float u11 = 1.0f;
    
    public float u12 = 1.0f;

    // Unity Editor bug:  Sliding this property with the mouse is slow.
    public float currentAngle = 0;
}
