using HSP;
using UnityEngine;

namespace TestMod
{
    public class Class1
    {
        [HSPEventListener( HSPEvent.STARTUP_IMMEDIATELY, "testmod.startup.immediately", Blacklist = new[] { "vanilla.something_random_12dafg6z18s" } )]
        public static void TestMethod( object e )
        {
            Debug.Log( "running test mod" );

            GameObject alwaysLoadedcustom = new GameObject( "alwaysLoadedcustom" );

            alwaysLoadedcustom.AddComponent<TestBeh>();
        }
    }

    public class TestBeh : MonoBehaviour
    {
        public float a;
    }
}
