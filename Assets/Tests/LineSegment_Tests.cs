using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Tests
{
    public class LineSegment_Tests {
        [Test]
        public void Intersect() {
            LineSegment seg = new LineSegment(new Vector2(6, -4), new Vector2(3, -3));
            Cup cup = new Cup(new Vector2(6, -3), new Vector2(3, -4), new Vector2(15, -7));
            Debug.Log(seg.Intersect(cup, .0001f));
        }
    }
}
