using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace HSP_Tests_EditMode
{
    public class E_Template
    {
        // ### ### ### ### ### ### ### ### ### ### ### ### ### ### ### ### ### ### ### ###

        // Tests use a fresh, clean scene.
        // If multiple tests are invoked at once, they will run in a single scene. Be careful.

        // ### ### ### ### ### ### ### ### ### ### ### ### ### ### ### ### ### ### ### ###

        // A Test behaves like an ordinary method
        [Test]
        public void CreatedGameObjectExists()
        {
            GameObject go = new GameObject( "hi" );

            Assert.AreEqual( go.name, "hi" );
            // Use the Assert class to test conditions
        }
    }
}