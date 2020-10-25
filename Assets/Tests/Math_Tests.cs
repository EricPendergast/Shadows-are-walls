using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Tests
{
    public class Math_Tests
    {
        // A Test behaves as an ordinary method
        [Test]
        public void AngleDifference() {
            Assert.AreEqual(Math.AngleDifference(-180, 180), 0);
            Assert.AreEqual(Math.AngleDifference(180, -180), 0);
            Assert.AreEqual(Math.AngleDifference(-360*20, -360*5), 0);
            Assert.AreEqual(Math.AngleDifference(-180-360, -179), 1);
        }
        [Test]
        public void LiesBetween() {
            Assert.IsFalse(Math.LiesBetween(20, 0, 360+10));
            Assert.IsTrue(Math.LiesBetween(5, 0, 360+10));
            Assert.IsTrue(Math.LiesBetween(180, 0, -1));
        }
        [Test]
        public void ClampAngle() {
            Assert.AreEqual(Math.ClampAngle(48, -47, 47), 47);
        }
        // A UnityTest behaves like a coroutine in Play Mode. In Edit Mode you can use
        // `yield return null;` to skip a frame.
        [UnityTest]
        public IEnumerator MathTestsWithEnumeratorPasses()
        {
            // Use the Assert class to test conditions.
            // Use yield to skip a frame.
            yield return null;
        }
    }
}
