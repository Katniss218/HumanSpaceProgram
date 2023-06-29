using System;
using UnityEngine;

namespace TestMod
{
    public class Class1
    {
        [KSS.Core.Mods.StaticEventListener( "startup.immediately", "testmod.startup.immediately", Blacklist = new[] { "vanilla.garbage" } )]
        public static void TestMethod( object obj )
        {
            UnityEngine.Debug.Log( "running test mod" );

            GameObject alwaysLoadedcustom = new GameObject( "alwaysLoadedcustom" );

            alwaysLoadedcustom.AddComponent<TestBeh>();
        }
    }

    public class TestBeh : MonoBehaviour
    {
        public float a;
    }
}
