using KSS.Core.Utils;
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
    public class ModManager : MonoBehaviour
    {
        // mods can be marked with an attribute that will be called at different points.

        private static Dictionary<HumanSpaceProgramInvokeAttribute.Startup, List<MethodInfo>> _modMethods = new Dictionary<HumanSpaceProgramInvokeAttribute.Startup, List<MethodInfo>>();


        private static void UpdateModMethods( Assembly a )
        {
            _modMethods = new Dictionary<HumanSpaceProgramInvokeAttribute.Startup, List<MethodInfo>>();
            foreach( HumanSpaceProgramInvokeAttribute.Startup w in Enum.GetValues( typeof( HumanSpaceProgramInvokeAttribute.Startup ) ) )
            {
                _modMethods[w] = new List<MethodInfo>();
            }

            Type[] types = a.GetTypes();
            foreach( var t in types )
            {
                MethodInfo[] methods = t.GetMethods( BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static );
                foreach( var m in methods )
                {
                    try
                    {
                        HumanSpaceProgramInvokeAttribute attr = m.GetCustomAttribute<HumanSpaceProgramInvokeAttribute>();
                        if( attr == null )
                        {
                            continue;
                        }

                        _modMethods[attr.WhenToRun].Add( m );
                    }
                    catch( TypeLoadException ex )
                    {
                        Debug.LogWarning( $"Couldn't resolve a type from the mod `{a.FullName}`: {ex.Message}." );
                    }
                }
            }
        }

        private static void LoadAssembliesRecursive( string path )
        {
            string[] dlls = Directory.GetFiles( path, "*.dll" );

            foreach( var dll in dlls )
            {
                byte[] file = File.ReadAllBytes( dll );
                Assembly modAssembly = Assembly.Load( file );

                UpdateModMethods( modAssembly );
            }

            string[] subfolders = Directory.GetDirectories( path );
            foreach( var subfolder in subfolders )
            {
                LoadAssembliesRecursive( subfolder );
            }
        }

        void Awake()
        {
            string dir = IOUtils.GetGameDataPath();

            if( !Directory.Exists( dir ) )
                Directory.CreateDirectory( dir );

            Debug.Log( $"Mod path: '{dir}'" );

            LoadAssembliesRecursive( dir );

            // methods marked with immediately should get invoked right at the start, in the always loaded scene.
            foreach( var m in _modMethods[HumanSpaceProgramInvokeAttribute.Startup.Immediately] )
            {
                m.Invoke( null, null );
            }
        }
    }
}