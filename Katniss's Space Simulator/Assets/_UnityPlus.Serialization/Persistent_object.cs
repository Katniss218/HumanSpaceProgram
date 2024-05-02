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

#warning TODO - move this stub from here to somewhere else.
            SerializedObject data = new SerializedObject()
            {
                { KeyNames.ID, s.GetID( obj ).GetData() },
                { KeyNames.TYPE, type.GetData() }
            };

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
            if( obj is IPersistsObjects p )
            {
                p.SetObjects( data, l ); // this can override auto-serialized members
            }
            else
            {
                PersistWithExtension.SetObjects( obj, obj.GetType(), data, l );
            }
        }

        public static SerializedData GetData( this object obj, IReverseReferenceMap s )
        {
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