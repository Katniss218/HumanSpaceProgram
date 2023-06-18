using System;
using UnityEngine;

namespace TestMod
{
    public class Class1
    {
        [KSS.Core.Mods.HumanSpaceProgramInvoke( KSS.Core.Mods.HumanSpaceProgramInvokeAttribute.Startup.Immediately )]
        public static void TestMethod()
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
