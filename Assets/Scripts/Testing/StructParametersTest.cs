using UnityEngine;

public class StructParametersTest : MonoBehaviour {
    public readonly struct MyVector2 {
        public readonly float x;
        public readonly float y;
        public MyVector2(float xIn, float yIn) {
            x = xIn;
            y = yIn;
        }
    }

    public static float CrossInVector2(in Vector2 v1, in Vector2 v2) {
        return v1.x*v2.y - v2.x*v1.y;
    }

    public static float CrossInMyVector2(in MyVector2 v1, in MyVector2 v2) {
        return v1.x*v2.y - v2.x*v1.y;
    }

    public static float CrossVector2(Vector2 v1, Vector2 v2) {
        return v1.x*v2.y - v2.x*v1.y;
    }

    public static float CrossMyVector2(MyVector2 v1, MyVector2 v2) {
        return v1.x*v2.y - v2.x*v1.y;
    }

    public static float Cross(float x1, float y1, float x2, float y2) {
        return x1*y2 - x2*y1;
    }

    public static float DoSomething() {
        float f = 12.5f;
        return f;
    }

    public int numIterations = 1000;

    void Update() {
        var v1 = new MyVector2(100, 20);
        var v2 = new MyVector2(10, 200);

        var v3 = new Vector2{x=100, y=20};
        var v4 = new Vector2{x=10, y=200};

        for (int i = 0; i < numIterations; i++) {
            CrossMyVector2(v1, v2);
            CrossInMyVector2(v1, v2);
            CrossVector2(v3, v4);
            CrossInVector2(v3, v4);
            Cross(v3.x, v3.y, v4.x, v4.y);
            DoSomething();
        }
    }
}
