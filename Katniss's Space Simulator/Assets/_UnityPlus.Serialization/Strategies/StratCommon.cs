using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using UnityPlus.Serialization.Json;

namespace UnityPlus.Serialization.Strategies
{
    /// <summary>
    /// Common utility methods for all strategies.
    /// </summary>
    public static class StratCommon
    {
        /// <summary>
        /// A noun for the file containing objects.
        /// </summary>
        public const string OBJECTS_NOUN = "objects";

        /// <summary>
        /// A noun for the file containing data about the objects.
        /// </summary>
        public const string OBJECTS_DATA_NOUN = "objects' data";

        /// <param name="noun">What the checked file contains.</param>
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static void ValidateFileOnLoad( string path, string noun )
        {
            if( string.IsNullOrEmpty( path ) )
            {
                throw new InvalidOperationException( $"Can't load {noun} - filepath is not set." );
            }
            if( !File.Exists( path ) )
            {
                throw new InvalidOperationException( $"Can't load {noun} - file `{path}` doesn't exist." );
            }
        }

        /// <param name="noun">What the checked file contains.</param>
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static void ValidateFileOnSave( string path, string noun )
        {
            if( string.IsNullOrEmpty( path ) )
            {
                throw new InvalidOperationException( $"Can't save {noun} - filepath is not set." );
            }
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static SerializedData ReadFromFile( string path )
        {
            string jsonContents = File.ReadAllText( path, Encoding.UTF8 );
            return new JsonStringReader( jsonContents ).Read();
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static void WriteToFile( string path, SerializedData data )
        {
            StringBuilder fileContents = new StringBuilder();
            new JsonStringWriter( data, fileContents ).Write();
            File.WriteAllText( path, fileContents.ToString(), Encoding.UTF8 );
        }
    }
}