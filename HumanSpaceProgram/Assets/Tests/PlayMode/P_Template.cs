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
        // If multiple tests are invoked at once, they will run in a single scene. Call each test separately.

        // ### ### ### ### ### ### ### ### ### ### ### ### ### ### ### ### ### ### ### ###

        // A UnityTest behaves like a coroutine in Play Mode. In Edit Mode you can use
        // `yield return new WaitForFixedUpdate();` to skip a frame (WARNING! - it resumes AFTER fixedupdate)
        // `yield return new WaitForSeconds( 1 );` to skip 1 second (WARNING! - it resumes AFTER update)
        // https://docs.unity3d.com/Manual/ExecutionOrder.html
        [UnityTest]
        public IEnumerator NewTestScriptWithEnumeratorPasses()
        {
            // Use the Assert class to test conditions.
            // Use yield to skip a frame.
            // NO NOT USE `yield return null;` - IT DOESN'T WORK IN TESTS (sometimes skips multiple frames, sometimes none at all!).
            yield return new WaitForFixedUpdate();
        }
    }
}