using System.Collections.Generic;
//using System.Linq;
using ListExtensions;
using UnityEngine;

using RunTest = EnumeratorTest;

public class GarbageCollectTest : MonoBehaviour {
    public RunTest test;
    void Awake() {
        test = new RunTest();
    }

    void Update() {
        test.Run();
    }

}

[System.Serializable]
public class EnumeratorTest {
    public int sum;
    public int min;

    public List<int> l = new List<int>();
    public void Run() {
        l.Clear();
        for (int i = 0; i < 1000; i++) {
            l.Add(i);
        }

        sum = 0;
        foreach (var item in l) {
            sum += item;
        }
        min = l.Min();

        l.RemoveAll(True);

        //Debug.Log(sum);
    }
    bool True(int i) {
        return true;
    }
}

[System.Serializable]
public class StructTest {
    public struct Thing {
        public int a;
        public double b;
        public string s;
    }

    public int sum;
    public List<Thing> l = new List<Thing>();

    public void Run() {
        l.Clear();
        for (int i = 0; i < 1000; i++) {
            l.Add(new Thing{a = i, s = "Hello"});
        }

        foreach (var t in l) {
            sum += t.a;
        }
    }
}
