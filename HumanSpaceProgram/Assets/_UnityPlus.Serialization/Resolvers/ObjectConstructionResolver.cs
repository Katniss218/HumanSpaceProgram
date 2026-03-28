using System;
using System.Reflection;
using System.Runtime.Serialization;

namespace UnityPlus.Serialization.Resolvers
{
    public enum ConstructionStrategy
    {
        None,
        DefaultConstructor,
        NonPublicConstructor,
        UninitializedObject
    }

    public static class ObjectConstructionResolver
    {
        public static (ConstructionStrategy strategy, Func<object> constructor) Resolve( Type type )
        {
            if( type.IsInterface || type.IsAbstract )
                return (ConstructionStrategy.None, null);

            ConstructorInfo ctor = type.GetConstructor( BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance, null, Type.EmptyTypes, null );
            if( ctor != null )
            {
                if( ctor.IsPublic )
                    return (ConstructionStrategy.DefaultConstructor, () => ctor.Invoke( null ));
                else
                    return (ConstructionStrategy.NonPublicConstructor, () => ctor.Invoke( null ));
            }

            if( type.IsValueType )
            {
                return (ConstructionStrategy.DefaultConstructor, () => Activator.CreateInstance( type ));
            }

            return (ConstructionStrategy.UninitializedObject, () => FormatterServices.GetUninitializedObject( type ));
        }
    }
}
