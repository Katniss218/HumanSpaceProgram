using System;

namespace TestMod
{
    public class Class1
    {
        [KSS.Core.Mods.ModStartup]
        public static void TestMethod()
        {
            UnityEngine.Debug.Log( "running test mod" );
        }
    }
}
