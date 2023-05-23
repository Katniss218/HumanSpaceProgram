using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace KSS.Tests.PlayMode
{
    public class P_Template
    {
        // ### ### ### ### ### ### ### ### ### ### ### ### ### ### ### ### ### ### ### ###

        // Tests use a fresh, clean scene.

        // ### ### ### ### ### ### ### ### ### ### ### ### ### ### ### ### ### ### ### ###

        // A Test behaves as an ordinary method
        [Test]
        public void NewTestScriptSimplePasses()
        {
            GameObject[] obj = GameObject.FindObjectsOfType<GameObject>();
            // Use the Assert class to test conditions
        }

        // A UnityTest behaves like a coroutine in Play Mode. In Edit Mode you can use
        // `yield return null;` to skip a frame.
        [UnityTest]
        public IEnumerator NewTestScriptWithEnumeratorPasses()
        {
            // Use the Assert class to test conditions.
            // Use yield to skip a frame.
            yield return null;
        }
    }
}