using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace UnityPlus.Serialization
{
    public static class ILoaderEx_Primitives
    {
        // Primitives in this context are types that are always saved in-place.

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static Vector2 ReadVector2( this ILoader _, SerializedData json )
        {
            return new Vector2( (float)json[0], (float)json[1] );
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static Vector2Int ReadVector2Int( this ILoader _, SerializedData json )
        {
            return new Vector2Int( (int)json[0], (int)json[1] );
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static Vector3 ReadVector3( this ILoader _, SerializedData json )
        {
            return new Vector3( (float)json[0], (float)json[1], (float)json[2] );
        }
        
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static Vector3Dbl ReadVector3Dbl( this ILoader _, SerializedData json )
        {
            return new Vector3Dbl( (double)json[0], (double)json[1], (double)json[2] );
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static Vector3Int ReadVector3Int( this ILoader _, SerializedData json )
        {
            return new Vector3Int( (int)json[0], (int)json[1], (int)json[2] );
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static Vector4 ReadVector4( this ILoader _, SerializedData json )
        {
            return new Vector4( (float)json[0], (float)json[1], (float)json[2], (float)json[3] );
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static Quaternion ReadQuaternion( this ILoader _, SerializedData json )
        {
            return new Quaternion( (float)json[0], (float)json[1], (float)json[2], (float)json[3] );
        }
        
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static QuaternionDbl ReadQuaternionDbl( this ILoader _, SerializedData json )
        {
            return new QuaternionDbl( (double)json[0], (double)json[1], (double)json[2], (double)json[3] );
        }

        /// <summary>
        /// Reads a Globally-Unique Identifier (GUID/UUID)
        /// </summary>
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static Guid ReadGuid( this ILoader _, SerializedData json )
        {
            // GUIDs should be saved in the '00000000-0000-0000-0000-000000000000' format.
            return Guid.ParseExact( (string)(SerializedPrimitive)json, "D" );
        }

        static Dictionary<string, Type> _stringToType = new Dictionary<string, Type>();

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static Type ReadType( this ILoader _, SerializedData json )
        {
            // 'AssemblyQualifiedName' is guaranteed to always uniquely identify a type.
            string assemblyQualifiedName = (string)(SerializedPrimitive)json;
            if( _stringToType.TryGetValue( assemblyQualifiedName, out Type type ) )
            {
                return type;
            }

            // Cache the type because accessing the Type.AssemblyQualifiedName and Type.GetType(string) is very slow.
            type = Type.GetType( assemblyQualifiedName );
            _stringToType.Add( assemblyQualifiedName, type );

            return type;
        }
    }
}