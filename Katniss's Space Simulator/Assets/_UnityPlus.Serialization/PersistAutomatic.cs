using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnityPlus.Serialization
{
    public interface IPersistsAutomatically
    {
        // just a marker.
    }

    [AttributeUsage( AttributeTargets.Field | AttributeTargets.Property )]
    public class PersistObjectAttribute : Attribute
    {
        // persistobject means it'll do whatever is being done inside the get/setobjects methods, but automatically.
    }

    [AttributeUsage( AttributeTargets.Field | AttributeTargets.Property )]
    public class PersistDataAttribute : Attribute
    {
        // persistdata means it'll do whatever is being done inside the get/setdata methods, but automatically.
    }

    public static class PersistAutomatic
    {
        private static bool _isInitialized = false;

        public static void ReloadMembers()
        {
            // TODO - actually reload the members.

            _isInitialized = true;
        }

        public static SerializedObject GetObjects( object obj, Type objType, IReverseReferenceMap s )
        {
            if( !_isInitialized )
                ReloadMembers();

            if( _extensionGetObjects.TryGetValue( objType, out var extensionMethod ) )
            {
                return (SerializedObject)extensionMethod.DynamicInvoke( obj, s );
            }
            return null;
        }

        public static void SetObjects( object obj, Type objType, SerializedObject data, IForwardReferenceMap l )
        {
            if( !_isInitialized )
                ReloadMembers();

            if( _extensionSetObjects.TryGetValue( objType, out var extensionMethod ) )
            {
                extensionMethod.DynamicInvoke( obj, data, l );
            }
        }

        public static SerializedData GetData( object obj, Type objType, IReverseReferenceMap s )
        {
            if( !_isInitialized )
                ReloadMembers();

            if( _extensionGetDatas.TryGetValue( objType, out var extensionMethod ) )
            {
                return (SerializedData)extensionMethod.DynamicInvoke( obj, s );
            }
            return null;
        }

        public static void SetData( object obj, Type objType, SerializedData data, IForwardReferenceMap l )
        {
            if( !_isInitialized )
                ReloadMembers();

            if( objType.IsValueType )
            {
                // pass by ref, if possible.
            }
            else
            {
                if( _extensionSetDatas.TryGetValue( objType, out var extensionMethod ) )
                {
                    extensionMethod.DynamicInvoke( obj, data, l );
                }
            }
        }
    }
}
