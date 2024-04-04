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

        public static object Create( SerializedObject data, IForwardReferenceMap l )
        {
            // this just instantiates.
            // call object.SetObjects( ... ) to deserialize the object parts.

            Type type = data[KeyNames.TYPE].ToType();
            Guid id = data[KeyNames.ID].ToGuid();

            object obj = null;
            if( _cache.TryGetClosest( type, out var factoryFunc ) )
            {
                obj = factoryFunc.Invoke( type, l );
            }
            else
            {
                obj = Activator.CreateInstance( type );
            }

            l.SetObj( id, obj );

            return obj;
        }
    }
}