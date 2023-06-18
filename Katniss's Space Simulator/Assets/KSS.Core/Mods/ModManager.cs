using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace KSS.Core.Mods
{
    [AttributeUsage( AttributeTargets.Method )]
    public class ModStartupAttribute : Attribute
    {

    }

    public class ModManager : MonoBehaviour
    {
        // mods can be marked with an attribute that will be called at different points.

        public static string GetExePath()
        {
            RuntimePlatform platform = Application.platform;
            string path = Application.dataPath;

            if( platform == RuntimePlatform.WindowsEditor || platform == RuntimePlatform.LinuxEditor || platform == RuntimePlatform.OSXEditor )
                return path + "/../";

            if( platform == RuntimePlatform.OSXPlayer )
                return path + "/../../";

            if( platform == RuntimePlatform.WindowsPlayer || platform == RuntimePlatform.LinuxPlayer )
                return path + "/../";

            return path;
        }

        private static MethodInfo[] GetModMethods( Assembly a )
        {
            List<MethodInfo> methodsToReturn = new List<MethodInfo>();

            Type[] types = a.GetTypes();
            foreach( var t in types )
            {
                MethodInfo[] methods = t.GetMethods( BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static );
                foreach( var m in methods )
                {
                    if( m.GetCustomAttribute<ModStartupAttribute>() != null )
                    {
                        methodsToReturn.Add( m );
                    }
                }
            }

            return methodsToReturn.ToArray();
        }

        private static void LoadAssemblies()
        {
            const string MOD_DIRECTORY = "GameData";

            string dir = Path.Combine( GetExePath(), MOD_DIRECTORY );

            if( !Directory.Exists( dir ) )
                Directory.CreateDirectory( dir );

            Debug.Log( $"Mod path: '{dir}'" );

            string[] dlls = Directory.GetFiles( dir, "*.dll" );

            foreach( var dll in dlls )
            {
                byte[] file = File.ReadAllBytes( dll );
                Assembly modAssembly = Assembly.Load( file );

                var methods = GetModMethods( modAssembly );
                foreach( var m in methods )
                {
                    m.Invoke( null, null );
                }
            }
        }

        void Awake()
        {
            LoadAssemblies();
        }
    }
}