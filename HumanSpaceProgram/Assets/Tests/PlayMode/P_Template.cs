using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace HSP_Tests_PlayMode
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
        // `yield return new WaitForFixedUpdate();` to skip a frame.
        // `yield return new WaitForSeconds( 1 );` to skip 1 second (WARNING! - it doesn't resume in the same place within the frame where it was paused).
        [UnityTest]
        public IEnumerator NewTestScriptWithEnumeratorPasses()
        {
            // Use the Assert class to test conditions.
            // Use yield to skip a frame.
            // NO NOT USE `yield return null;` - IT DOESN'T WORK IN TESTS.
            yield return new WaitForFixedUpdate();
        }
    }
}