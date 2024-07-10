using System;
using UnityEngine;

namespace TestMod
{
    public class Class1
    {
        [HSP.Core.HSPEventListener( HSP.Core.HSPEvent.STARTUP_IMMEDIATELY, "testmod.startup.immediately", Blacklist = new[] { "vanilla.something_random_12dafg6z18s" } )]
        public static void TestMethod( object e )
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
