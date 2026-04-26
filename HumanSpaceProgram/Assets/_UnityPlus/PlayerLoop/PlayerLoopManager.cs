using System;
using System.Collections.Generic;
using System.Reflection;

namespace UnityPlus.PlayerLoop
{
    /// <summary>
    /// Manages the player loop, providing methods to initialize it with custom systems and groups, and to scan assemblies for types decorated with player loop attributes.
    /// </summary>
    public static class PlayerLoopManager
    {
        public static void Initialize( BucketHandling handling = BucketHandling.IncludeSkip )
        {
            PlayerLoopCompiler compiler = new PlayerLoopCompiler();

            UnityEngine.LowLevel.PlayerLoopSystem nativeLoop = UnityEngine.LowLevel.PlayerLoop.GetCurrentPlayerLoop();
            List<Type> nodes = ScanAllAssemblies();

            UnityEngine.LowLevel.PlayerLoopSystem newLoop = compiler.Compile( nativeLoop, nodes, handling );

            UnityEngine.LowLevel.PlayerLoop.SetPlayerLoop( newLoop );
        }

        public static List<Type> ScanAllAssemblies()
        {
            List<Type> allTypes = new();
            foreach( var assembly in AppDomain.CurrentDomain.GetAssemblies() )
            {
                Type[] types;
                try
                {
                    types = assembly.GetTypes();
                }
                catch( ReflectionTypeLoadException e )
                {
                    types = e.Types;
                }

                if( types == null )
                    continue;

                foreach( var type in types )
                {
                    if( type == null )
                        continue;
                    if( Attribute.IsDefined( type, typeof( PlayerLoopSystemAttribute ), inherit: false ) ||
                        Attribute.IsDefined( type, typeof( PlayerLoopNativeAttribute ), inherit: false ) )
                    {
                        allTypes.Add( type );
                    }
                }
            }

            return allTypes;
        }
    }
}