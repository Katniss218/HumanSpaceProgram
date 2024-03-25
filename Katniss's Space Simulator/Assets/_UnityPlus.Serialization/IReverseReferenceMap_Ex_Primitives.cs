using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace UnityPlus.Serialization
{
	public static class IReverseReferenceMap_Ex_Primitives
	{
		public static SerializedArray GetData<T>( this IEnumerable<T> e, IReverseReferenceMap s )
		{
			SerializedArray array = new SerializedArray();
			foreach( var item in e )
			{
				array.Add( item.GetData( s ) );
			}
			return array;
		}


		public static SerializedData GetData( this Vector2 v )
		{
			return new SerializedArray() { (SerializedPrimitive)v.x, (SerializedPrimitive)v.y };
		}

		public static Vector2 ToVector2( this SerializedData array ) 
		{
            return new Vector2( (float)array[0], (float)array[1] );
		}

		//


		// Primitives in this context are types that are always saved in-place.

		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public static SerializedArray WriteVector2( this IReverseReferenceMap _, Vector2 v )
		{
			return new SerializedArray() { (SerializedPrimitive)v.x, (SerializedPrimitive)v.y };
		}

		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public static SerializedArray WriteVector2Int( this IReverseReferenceMap _, Vector2Int v )
		{
			return new SerializedArray() { (SerializedPrimitive)v.x, (SerializedPrimitive)v.y };
		}

		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public static SerializedArray WriteVector3( this IReverseReferenceMap _, Vector3 v )
		{
			return new SerializedArray() { (SerializedPrimitive)v.x, (SerializedPrimitive)v.y, (SerializedPrimitive)v.z };
		}

		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public static SerializedArray WriteVector3Dbl( this IReverseReferenceMap _, Vector3Dbl v )
		{
			return new SerializedArray() { (SerializedPrimitive)v.x, (SerializedPrimitive)v.y, (SerializedPrimitive)v.z };
		}

		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public static SerializedArray WriteVector3Int( this IReverseReferenceMap _, Vector3Int v )
		{
			return new SerializedArray() { (SerializedPrimitive)v.x, (SerializedPrimitive)v.y, (SerializedPrimitive)v.z };
		}

		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public static SerializedArray WriteVector4( this IReverseReferenceMap _, Vector4 v )
		{
			return new SerializedArray() { (SerializedPrimitive)v.x, (SerializedPrimitive)v.y, (SerializedPrimitive)v.z, (SerializedPrimitive)v.w };
		}

		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public static SerializedArray WriteQuaternion( this IReverseReferenceMap _, Quaternion q )
		{
			return new SerializedArray() { (SerializedPrimitive)q.x, (SerializedPrimitive)q.y, (SerializedPrimitive)q.z, (SerializedPrimitive)q.w };
		}

		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public static SerializedArray WriteQuaternionDbl( this IReverseReferenceMap _, QuaternionDbl q )
		{
			return new SerializedArray() { (SerializedPrimitive)q.x, (SerializedPrimitive)q.y, (SerializedPrimitive)q.z, (SerializedPrimitive)q.w };
		}

		/// <summary>
		/// Writes a Globally-Unique Identifier (GUID/UUID)
		/// </summary>
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public static SerializedPrimitive WriteGuid( this IReverseReferenceMap _, Guid value )
		{
			// GUIDs should be saved in the '00000000-0000-0000-0000-000000000000' format.
			return (SerializedPrimitive)value.ToString( "D" );
		}

		static readonly Dictionary<Type, string> _typeToString = new Dictionary<Type, string>();

		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public static SerializedPrimitive WriteType( this IReverseReferenceMap _, Type value )
		{
			// 'AssemblyQualifiedName' is guaranteed to always uniquely identify a type.
			if( _typeToString.TryGetValue( value, out string assemblyQualifiedName ) )
			{
				return (SerializedPrimitive)assemblyQualifiedName;
			}

			// Cache the type because accessing the Type.AssemblyQualifiedName and Type.GetType(string) is very slow.
			assemblyQualifiedName = value.AssemblyQualifiedName;
			_typeToString.Add( value, assemblyQualifiedName );

			return (SerializedPrimitive)assemblyQualifiedName;
		}
	}
}