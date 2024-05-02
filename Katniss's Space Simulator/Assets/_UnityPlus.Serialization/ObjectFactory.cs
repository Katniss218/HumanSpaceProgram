using System;
using UnityEngine;

namespace UnityPlus.Serialization
{
    public static class ObjectFactory
    {
        private static readonly TypeMap<Func<Type, IForwardReferenceMap, object>> _cache = new();

        public static void ReloadFactoryMethods()
        {
            // Load extension methods `Object ToObject( this SerializedData, IForwardReferenceMap l )`, where 'Object' is the identifier of the type in question, with special chars replaced by `_`.

            // this should also handle immutables.
        }

        public static SerializedObject WriteObjectStub( object obj, IReverseReferenceMap s )
        {
            return new SerializedObject()
            {
                { KeyNames.REF, s.GetID( obj ).GetData() },
            };
        }

        public static SerializedObject WriteObjectStubTyped( object obj, Type objType, IReverseReferenceMap s )
        {
            return new SerializedObject()
            {
                { KeyNames.ID, s.GetID( obj ).GetData() },
                { KeyNames.TYPE, objType.GetData() }
            };
        }

        public static object ToObject( this SerializedObject data, IForwardReferenceMap l )
        {
            Type type = data[KeyNames.TYPE].ToType();
            Guid id = data[KeyNames.ID].ToGuid();

            object obj = null;
            if( _cache.TryGetClosest( type, out var factoryFunc ) )
            {
                // Factory func should add everything it creates to the reference map.
                obj = factoryFunc.Invoke( type, l );
            }
            else
            {
                obj = Activator.CreateInstance( type );
                l.SetObj( id, obj );
            }

            return obj;
        }
    }
}