using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace UnityPlus.Serialization
{
    public static class Persistent_object
    {
        //
        //  OBJECTS
        //

        public static SerializedObject WriteObject<T>( this IReverseReferenceMap s, T obj ) where T : class
        {
            // Writes object stub + any owned objects.

            Type objType = typeof( T );

            SerializedObject data = new SerializedObject()
            {
                { KeyNames.ID, s.GetID( obj ).GetData() }
            };

            if( obj is IPersistsObjects p )
            {
#warning TODO - this will call the actual type's method, instead of the base type T.
                SerializedObject ownsMap = p.GetObjects( s );
                if( ownsMap != null )
                {
                    foreach( var kvp in ownsMap )
                    {
                        data.Add( kvp.Key, kvp.Value );
                    }
                }
            }
            else
            {
                SerializedObject ownsMap = PersistWithExtension.GetObjects( obj, objType, s );
                if( ownsMap != null )
                {
                    foreach( var kvp in ownsMap )
                    {
                        data.Add( kvp.Key, kvp.Value );
                    }
                }
            }

            return data;
        }

        public static SerializedObject WriteObjectTyped<T>( this IReverseReferenceMap s, T obj ) where T : class
        {
            Type objType = obj.GetType();

            SerializedObject data = new SerializedObject()
            {
                { KeyNames.ID, s.GetID( obj ).GetData() },
                { KeyNames.TYPE, objType.GetData() }
            };

            if( obj is IPersistsObjects p )
            {
                SerializedObject ownsMap = p.GetObjects( s );
                if( ownsMap != null )
                {
                    foreach( var kvp in ownsMap )
                    {
                        data.Add( kvp.Key, kvp.Value );
                    }
                }
            }
            else
            {
                SerializedObject ownsMap = PersistWithExtension.GetObjects( obj, objType, s );
                if( ownsMap != null )
                {
                    foreach( var kvp in ownsMap )
                    {
                        data.Add( kvp.Key, kvp.Value );
                    }
                }
            }

            return data;
        }

        /// <summary>
        /// Inverse of WriteObject
        /// </summary>
        public static T ReadObject<T>( this IForwardReferenceMap l, SerializedData data )
        {
            T obj = ObjectFactory.CreateObject<T>( data, l ); // this just finds and calls the factory (or the default factory) (factory includes registering the ID, if ID is found).

            if( data is SerializedObject dataObj )
            {
                obj.SetObjects( dataObj, l );
            }

            return obj;
        }

        //
        //  OBJECTS
        //

        public static SerializedObject GetObjects( this object obj, IReverseReferenceMap s )
        {
            switch( obj )
            {
                case IPersistsObjects o:
                    return o.GetObjects( s );
                case IPersistsAutomatically:
                    return PersistAutomatic.GetObjects( obj, obj.GetType(), s );
                default:
                    return PersistWithExtension.GetObjects( obj, obj.GetType(), s );
            }
        }

        public static void SetObjects( this object obj, SerializedObject data, IForwardReferenceMap l )
        {
            switch( obj )
            {
                case IPersistsObjects o:
                    o.SetObjects( data, l );
                    break;
                case IPersistsAutomatically:
                    PersistAutomatic.SetObjects( obj, obj.GetType(), data, l );
                    break;
                default:
                    PersistWithExtension.SetObjects( obj, obj.GetType(), data, l );
                    break;
            }
        }

        //
        //  DATA
        //

        public static SerializedData GetData( this object obj, IReverseReferenceMap s )
        {
            switch( obj )
            {
                case IPersistsData o:
                    return o.GetData( s );
                case IPersistsAutomatically:
                    return PersistAutomatic.GetData( obj, obj.GetType(), s );
                default:
                    return PersistWithExtension.GetData( obj, obj.GetType(), s );
            }
        }

        public static void SetData( this object obj, SerializedData data, IForwardReferenceMap l )
        {
            switch( obj )
            {
                case IPersistsData o:
                    o.SetData( data, l );
                    break;
                case IPersistsAutomatically:
                    PersistAutomatic.SetData( obj, obj.GetType(), data, l );
                    break;
                default:
                    PersistWithExtension.SetData( obj, obj.GetType(), data, l );
                    break;
            }
        }

        //
        //  INLINE
        //

        //public static SerializedObject WriteObjectInline<T>( this IReverseReferenceMap s, T obj )
        public static SerializedObject AsSerializedInline<T>( this T obj, IReverseReferenceMap s )
        {

        }

        //public static SerializedObject WriteObjectInlineTyped<T>( this IReverseReferenceMap s, T obj )
        public static SerializedObject AsSerializedInlineTyped<T>( this T obj, IReverseReferenceMap s )
        {

        }


        public static T ReadObjectInline<T>( this IForwardReferenceMap l, SerializedObject data )
        {
            T obj = ObjectFactory.AsObjectInline<T>( data["value"], l );
#warning TODO - ignore the type field in the factory here.

#warning TODO - objects might not have a `setdata`/setobjects method, and deserialize directly.
            /*if( obj is IPersistsObjects p )
            {
                p.SetObjects( data, l );
            }
            else
            {
                PersistWithExtension.SetObjects( obj, obj.GetType(), data, l );
            }

            switch( obj )
            {
                case IPersistsData o:
                    o.SetData( data, l );
                    break;
                default:
                    PersistWithExtension.SetData( obj, obj.GetType(), l, data );
                    break;
            }*/

            return obj;
        }

        public static T ReadObjectInlineTyped<T>( this IForwardReferenceMap l, SerializedObject data )
        {
            T obj = ObjectFactory.AsObjectInline<T>( data, l );

            return obj;
        }

        public static SerializedData AsSerialized<T>( this T obj, IReverseReferenceMap s )
        {
            // This is a general version of AsSerialized extension method when called on a nongeneral type.


        }

        /// <summary>
        /// Creates an instance using the type that has been serialized when the data was written.
        /// </summary>
        /// <remarks>
        /// The instance creation is cascading, any instances that were owned by the serialized instance will also be created. <br/>
        /// The serialized type must be a subtype of T, or implementing T (if T is an interface).
        /// </remarks>
        /// <returns>
        /// The newly created instance.
        /// </returns>
        public static T AsObject<T>( this SerializedData data, IForwardReferenceMap l )
        {
            // This is a general version of AsString / AsGameObject / AsSequenceElement / etc.
            // Usage with normal types: `AsObject<string>`
            // Usage with generic types: `AsObject<List<string>>`

            // This just does whatever the factory does.

            T obj = ObjectFactory.CreateObject<T>( data, l ); // this just finds and calls the factory (or the default factory).

            return obj;
        }
    }
}