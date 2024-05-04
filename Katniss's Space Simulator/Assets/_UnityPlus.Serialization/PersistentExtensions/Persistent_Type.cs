using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace UnityPlus.Serialization
{
	public static class Persistent_Type
	{
		private static readonly Dictionary<Type, string> _typeToString = new();
		private static readonly Dictionary<string, Type> _stringToType = new();

		// I'm caching the type and its string representation because accessing the Type.AssemblyQualifiedName and Type.GetType(string) is very slow.

		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public static SerializedPrimitive GetData( this Type type, IReverseReferenceMap s = null )
		{
			if( _typeToString.TryGetValue( type, out string assemblyQualifiedName ) )
			{
				return (SerializedPrimitive)assemblyQualifiedName;
			}
			
			// 'AssemblyQualifiedName' is guaranteed to always uniquely identify a type.
			assemblyQualifiedName = type.AssemblyQualifiedName;
			_typeToString.Add( type, assemblyQualifiedName );

			return (SerializedPrimitive)assemblyQualifiedName;
		}

		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public static Type AsType( this SerializedData data, IForwardReferenceMap l = null )
		{
			string assemblyQualifiedName = data.AsString();
			if( _stringToType.TryGetValue( assemblyQualifiedName, out Type type ) )
			{
				return type;
			}
			
			// 'AssemblyQualifiedName' is guaranteed to always uniquely identify a type.
			type = Type.GetType( assemblyQualifiedName );
			_stringToType.Add( assemblyQualifiedName, type );

			return type;
		}
	}
}