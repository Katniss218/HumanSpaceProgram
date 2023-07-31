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
    /// <summary>
    /// Knows how to load mods from a mod directory.
    /// </summary>
    public static class ModLoader
    {
        public static string GetModDirectory() => HumanSpaceProgram.GetGameDataPath();

        private static void LoadAssembliesRecursive( string path )
        {
            foreach( var dllPath in Directory.GetFiles( path, "*.dll" ) )
            {
                byte[] assemblyBytes = File.ReadAllBytes( dllPath );
                Assembly.Load( assemblyBytes );
            }

            foreach( var subfolder in Directory.GetDirectories( path ) )
            {
                LoadAssembliesRecursive( subfolder );
            }
        }

        /// <summary>
        /// Loads all of the assemblies (.dll) in the mod directory.
        /// </summary>
        internal static void LoadModAssemblies()
        {
            string modDirectory = GetModDirectory();

            if( !Directory.Exists( modDirectory ) )
                Directory.CreateDirectory( modDirectory );

            Debug.Log( $"The mod directory is: '{modDirectory}'" );

            LoadAssembliesRecursive( modDirectory );
        }
    }
}