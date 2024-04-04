using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace UnityPlus.Serialization
{
    public static class Persistent_object
    {
        public static SerializedObject GetObjects( this object obj, IReverseReferenceMap s )
        {
            Type type = obj.GetType();

            SerializedObject data = new SerializedObject()
            {
                { KeyNames.ID, s.GetID( obj ).GetData() },
                { KeyNames.TYPE, type.GetData() }
            };

            /*if( obj is IAutoPersistsObjects )
            {
                SerializedObject ownsMap = PersistsAutomatic.GetObjects( obj, type, s );

                foreach( var kvp in ownsMap )
                {
                    data.Add( kvp.Key, kvp.Value );
                }
            }*/

            if( obj is IPersistsObjects p )
            {
                SerializedObject ownsMap = p.GetObjects( s ); // this can override auto-serialized members
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
                SerializedObject ownsMap = PersistWithExtension.GetObjects( obj, type, s );
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

        public static void SetObjects( this object obj, SerializedObject data, IForwardReferenceMap l )
        {
            /*if( obj is IAutoPersistsObjects )
            {
                PersistsAutomatic.SetObjects( obj, obj.GetType(), data, l );
            }*/

            if( obj is IPersistsObjects p )
            {
                p.SetObjects( data, l ); // this can override auto-serialized members
            }
            else
            {
                PersistWithExtension.SetObjects( obj, obj.GetType(), data, l );
            }
        }

        // TODO - For get/setdata of derived objects, we actually want to call the get/setdata of every base type as well. Including if that type is not "ours".
               // The use case is to enable serialization of base fields/properties without having access to the base type (e.g. for `enabled` from UnityEngine.Behaviour).

        // hmm, since when overriding, we're using 'base.X()', maybe just call to serialize/deserialize the base thing.

        public static SerializedData GetData( this object obj, IReverseReferenceMap s )
        {
            /*if( obj is IAutoPersistsData )
            {
                var rootSO = PersistsAutomatic.GetData( obj, obj.GetType(), s );

                return rootSO; // TODO - combine with rest.
            }*/

            switch( obj )
            {
                case IPersistsData o:
                    return o.GetData( s );
                default:
                    return PersistWithExtension.GetData( obj, obj.GetType(), s );
            }
        }

        public static void SetData( this object obj, SerializedData data, IForwardReferenceMap l )
        {
            /*if( obj is IAutoPersistsData )
            {
                PersistsAutomatic.SetData( obj, obj.GetType(), l, data );
            }*/

            switch( obj )
            {
                case IPersistsData o:
                    o.SetData( data, l ); break;
                default:
                    PersistWithExtension.SetData( obj, obj.GetType(), l, data ); break;
            }
        }
    }
}