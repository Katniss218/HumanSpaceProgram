using KSS.Core.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityPlus.StaticEvents;

namespace KSS.Core.Mods
{
    public class ModManager : MonoBehaviour
    {
        public static OverridableEventManager StaticEvent { get; private set; } = new OverridableEventManager();

        private static void CacheAutoRunMethods( Assembly a )
        {
            Type[] types = a.GetTypes();
            foreach( var t in types )
            {
                MethodInfo[] methods = t.GetMethods( BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static );
                foreach( var method in methods )
                {
                    try
                    {
                        StaticEventListenerAttribute attr = method.GetCustomAttribute<StaticEventListenerAttribute>();
                        if( attr == null )
                        {
                            continue;
                        }

                        var parameters = method.GetParameters();

                        if( parameters.Length != 1 || parameters[0].ParameterType != typeof( object ) || method.ReturnType != typeof( void ) )
                        {
                            Debug.LogWarning( $"Ignoring a `{nameof( StaticEventListenerAttribute )}` attribute applied to method `{method.Name}` which doesn't follow the signature `void Method( object obj )`." );
                            continue;
                        }

                        Action<object> methodDelegate = (Action<object>)Delegate.CreateDelegate( typeof( Action<object> ), method );

                        StaticEvent.TryCreate( attr.EventID );
                        StaticEvent.TryAddListener( attr.EventID, new OverridableEventListener<Action<object>>() { id = attr.ID, blacklist = attr.Blacklist, func = methodDelegate } );
                    }
                    catch( TypeLoadException ex )
                    {
                        Debug.LogWarning( $"Couldn't resolve a type from the mod `{a.FullName}`: {ex.Message}." );
                    }
                }
            }
        }

        private static void LoadAssembliesRecursive( string path, Action<Assembly> del )
        {
            string[] dlls = Directory.GetFiles( path, "*.dll" );

            foreach( var dll in dlls )
            {
                byte[] file = File.ReadAllBytes( dll );
                Assembly modAssembly = Assembly.Load( file );

                del?.Invoke( modAssembly );
            }

            string[] subfolders = Directory.GetDirectories( path );
            foreach( var subfolder in subfolders )
            {
                LoadAssembliesRecursive( subfolder, del );
            }
        }

        void Awake()
        {
            string dir = IOUtils.GetGameDataPath();

            if( !Directory.Exists( dir ) )
                Directory.CreateDirectory( dir );

            Debug.Log( $"Mod path: '{dir}'" );

            LoadAssembliesRecursive( dir, ( modAssembly ) => CacheAutoRunMethods( modAssembly ) );

            // methods marked with immediately should get invoked right at the start, in the always loaded scene.

            StaticEvent.TryInvoke( "startup.immediately" );
        }
    }
}