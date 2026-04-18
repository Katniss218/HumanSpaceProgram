using System;
using System.Reflection;

namespace UnityPlus.Serialization
{
    /// <summary>
    /// Explicitly tells the TypeDescriptorRegistry to bind this generic parameter to the exact Target Type being processed, 
    /// rather than mapping the deconstructed generic elements or using automatic heuristics.
    /// </summary>
    [AttributeUsage( AttributeTargets.GenericParameter )]
    public class BindTargetAttribute : Attribute
    {
    }
    public static class ProviderArgsResolver
    {
        public static Type[] GetDeconstructedArgs( Type targetType, Type matchedType )
        {
            if( matchedType != null && matchedType.IsGenericTypeDefinition )
            {
                Type instantiation = FindGenericInstantiation( targetType, matchedType );
                if( instantiation != null )
                    return instantiation.GetGenericArguments();
            }
            else if( matchedType == typeof( Array ) || targetType.IsArray )
            {
                return new Type[] { targetType.GetElementType() };
            }
            else if( targetType.IsGenericType )
            {
                return targetType.GetGenericArguments();
            }
            else if( targetType.IsEnum )
            {
                return new Type[] { targetType };
            }

            return Type.EmptyTypes;
        }

        private static Type FindGenericInstantiation( Type type, Type genericDefinition )
        {
            if( genericDefinition.IsInterface )
            {
                if( type.IsInterface && type.IsGenericType && type.GetGenericTypeDefinition() == genericDefinition )
                    return type;

                foreach( var itf in type.GetInterfaces() )
                {
                    if( itf.IsGenericType && itf.GetGenericTypeDefinition() == genericDefinition )
                        return itf;
                }
            }
            else
            {
                var curr = type;
                while( curr != null )
                {
                    if( curr.IsGenericType && curr.GetGenericTypeDefinition() == genericDefinition )
                        return curr;
                    curr = curr.BaseType;
                }
            }
            return null;
        }
    }
    public static class ProviderBindingUtility
    {
        public static MethodInfo Bind( MethodInfo rawMethod, Type targetType, Type[] deconstructedArgs )
        {
            // Retrieve generic parameters defined by the provider
            Type[] providerGenericParams = rawMethod.DeclaringType != null && rawMethod.DeclaringType.IsGenericTypeDefinition
                ? rawMethod.DeclaringType.GetGenericArguments()
                : rawMethod.IsGenericMethodDefinition ? rawMethod.GetGenericArguments() : Type.EmptyTypes;

            if( providerGenericParams.Length == 0 )
                return rawMethod; // Nothing to bind

            if( deconstructedArgs == null )
                deconstructedArgs = Type.EmptyTypes;

            // Map generic parameters to concrete types
            Type[] resolvedArgs = new Type[providerGenericParams.Length];
            int deconstructedIndex = 0;

            for( int i = 0; i < providerGenericParams.Length; i++ )
            {
                var param = providerGenericParams[i];
                if( param.IsDefined( typeof( BindTargetAttribute ), false ) )
                {
                    resolvedArgs[i] = targetType;
                }
                else
                {
                    if( deconstructedIndex < deconstructedArgs.Length )
                    {
                        resolvedArgs[i] = deconstructedArgs[deconstructedIndex++];
                    }
                    else
                    {
                        // Could not fulfill all non-[BindTarget] parameters safely.
                        return null;
                    }
                }
            }

            // CASE 1: The Provider is inside a Generic Class (e.g. class Provider<T> { static Method() } )
            if( rawMethod.DeclaringType != null && rawMethod.DeclaringType.IsGenericTypeDefinition )
            {
                try
                {
                    Type closedProviderType = rawMethod.DeclaringType.MakeGenericType( resolvedArgs );
                    return (MethodInfo)MethodBase.GetMethodFromHandle( rawMethod.MethodHandle, closedProviderType.TypeHandle );
                }
                catch( ArgumentException )
                {
                    return null; // Constraint validation failure
                }
            }
            // CASE 2: The Provider Method itself is Generic (e.g. static Method<T>() )
            else if( rawMethod.IsGenericMethodDefinition )
            {
                try
                {
                    if( rawMethod.GetGenericArguments().Length == resolvedArgs.Length )
                    {
                        return rawMethod.MakeGenericMethod( resolvedArgs );
                    }
                }
                catch( ArgumentException )
                {
                    return null; // Constraint validation failure
                }
            }

            return rawMethod;
        }
    }
}