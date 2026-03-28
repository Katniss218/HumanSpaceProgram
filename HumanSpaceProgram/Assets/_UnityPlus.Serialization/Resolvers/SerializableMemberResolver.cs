using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityPlus.Serialization.Descriptors;

namespace UnityPlus.Serialization.Resolvers
{
    public static class SerializableMemberResolver
    {
        public static IMemberInfo[] GetMembers( Type type )
        {
            var memberList = new List<IMemberInfo>();
            var seenNames = new HashSet<string>();

            Type currentType = type;
            while( currentType != null && currentType != typeof( object ) )
            {
                var fields = currentType.GetFields( BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly );

                foreach( var field in fields )
                {
                    if( field.IsStatic )
                        continue;

                    bool isPublic = field.IsPublic;
                    bool hasSerializeField = field.GetCustomAttribute<SerializeField>() != null;
                    bool hasNonSerialized = field.GetCustomAttribute<NonSerializedAttribute>() != null;

                    if( field.Name.Contains( "<" ) || field.Name.Contains( ">" ) )
                    {
                        if( !hasSerializeField )
                            continue; // Ignore compiler-generated backing fields unless explicitly serialized
                    }

                    if( hasNonSerialized )
                        continue;
                    if( !isPublic && !hasSerializeField )
                        continue;

                    // Handle shadowing: if a derived class hides a base class field, we might want to serialize both or just the derived one.
                    // Unity serializes both, but they might have the same name.
                    // For now, let's just add them.
                    if( seenNames.Add( field.Name ) )
                    {
                        try
                        {
                            memberList.Add( new ReflectionFieldInfo( field ) );
                        }
                        catch( Exception ex )
                        {
                            Debug.LogError( $"Failed to create ReflectionFieldInfo for field '{field.Name}' in type '{currentType.FullName}': {ex.Message}" );
                            throw;
                        }
                    }
                }

                var properties = currentType.GetProperties( BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly );

                foreach( var property in properties )
                {
                    if( !property.CanRead || !property.CanWrite )
                        continue;

                    if( property.GetIndexParameters().Length > 0 )
                        continue;

                    bool isPublic = property.GetMethod != null && property.GetMethod.IsPublic && property.SetMethod != null && property.SetMethod.IsPublic;
                    bool hasSerializeField = property.GetCustomAttribute<SerializeField>() != null;
                    bool hasNonSerialized = property.GetCustomAttribute<NonSerializedAttribute>() != null;

                    if( hasNonSerialized )
                        continue;
                    if( !isPublic && !hasSerializeField )
                        continue;

                    if( seenNames.Add( property.Name ) )
                    {
                        try
                        {
                            memberList.Add( new ReflectionPropertyInfo( property ) );
                        }
                        catch( Exception ex )
                        {
                            Debug.LogError( $"Failed to create ReflectionFieldInfo for field '{property.Name}' in type '{currentType.FullName}': {ex.Message}" );
                            throw;
                        }
                    }
                }

                currentType = currentType.BaseType;
            }

            return memberList.ToArray();
        }
    }
}
