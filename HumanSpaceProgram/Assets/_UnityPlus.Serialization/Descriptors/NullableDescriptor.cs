using System;
using System.Collections.Generic;

namespace UnityPlus.Serialization
{
    /// <summary>
    /// A descriptor that handles Nullable{T} by delegating to the descriptor of T.
    /// </summary>
    public class NullableDescriptor<T> : ICompositeDescriptor where T : struct
    {
        private readonly IDescriptor _underlyingDescriptor;

        public Type MappedType => typeof( T? );

        public NullableDescriptor( ContextKey context )
        {
            _underlyingDescriptor = TypeDescriptorRegistry.GetDescriptor( typeof( T ), context );
        }

        public object CreateInitialTarget( SerializedData data, SerializationContext context )
        {
            // Nullable<T> is a struct, but we can return null here if the data is null.
            // However, CreateInitialTarget is usually called for non-null data.
            // If data is null, the strategy handles it before calling us.

            // If we have data, we delegate to the underlying type to create the initial value.
            if( _underlyingDescriptor is ICompositeDescriptor comp )
            {
                return comp.CreateInitialTarget( data, context );
            }

            // If underlying is primitive, we shouldn't be here (Primitives are handled by IPrimitiveDescriptor)
            // But wait, Nullable<int> is a struct, so it might come here.
            // If T is primitive, Nullable<T> is NOT a primitive in the serialization sense (it's a struct).
            // But we want it to behave like one if T is primitive.

            return Activator.CreateInstance<T>();
        }

        public object Construct( object initialTarget )
        {
            if( _underlyingDescriptor is ICompositeDescriptor comp )
            {
                return comp.Construct( initialTarget );
            }
            return Activator.CreateInstance<T>();
        }

        public int GetConstructionStepCount( object target )
        {
            if( target == null ) return 0;
            if( _underlyingDescriptor is ICompositeDescriptor comp )
            {
                return comp.GetConstructionStepCount( target );
            }
            return 0;
        }

        public int GetStepCount( object target )
        {
            if( target == null ) return 0;
            if( _underlyingDescriptor is ICompositeDescriptor comp )
            {
                return comp.GetStepCount( target );
            }
            return 0; // Should not happen if T is composite
        }

        public IEnumerator<IMemberInfo> GetMemberEnumerator( object target )
        {
            if( target == null )
                return null;
            if( _underlyingDescriptor is ICompositeDescriptor comp )
            {
                return comp.GetMemberEnumerator( target );
            }
            return null;
        }

        public IMemberInfo GetMemberInfo( int index )
        {
            if( _underlyingDescriptor is ICompositeDescriptor comp )
            {
                return comp.GetMemberInfo( index );
            }
            return null;
        }

        public int GetMethodCount()
        {
            if( _underlyingDescriptor is ICompositeDescriptor comp )
            {
                return comp.GetMethodCount();
            }
            return 0;
        }

        public IMethodInfo GetMethodInfo( int methodIndex )
        {
            if( _underlyingDescriptor is ICompositeDescriptor comp )
            {
                return comp.GetMethodInfo( methodIndex );
            }
            return null;
        }

        public void OnSerializing( object target, SerializationContext context )
        {
            if( target != null && _underlyingDescriptor is ICompositeDescriptor comp )
            {
                comp.OnSerializing( target, context );
            }
        }

        public void OnDeserialized( object target, SerializationContext context )
        {
            if( target != null && _underlyingDescriptor is ICompositeDescriptor comp )
            {
                comp.OnDeserialized( target, context );
            }
        }
    }

    /// <summary>
    /// Handles Nullable{T} where T is a primitive (e.g. int?, float?).
    /// </summary>
    public class NullablePrimitiveDescriptor<T> : IPrimitiveDescriptor where T : struct
    {
        private readonly IPrimitiveDescriptor _underlyingDescriptor;

        public Type MappedType => typeof( T? );

        public NullablePrimitiveDescriptor( ContextKey context )
        {
            _underlyingDescriptor = (IPrimitiveDescriptor)TypeDescriptorRegistry.GetDescriptor( typeof( T ), context );
        }

        public void SerializeDirect( object target, ref SerializedData data, SerializationContext ctx )
        {
            if( target == null )
            {
                data = null;
                return;
            }
            _underlyingDescriptor.SerializeDirect( target, ref data, ctx );
        }

        public DeserializationResult DeserializeDirect( SerializedData data, SerializationContext ctx, out object result )
        {
            if( data == null )
            {
                result = null;
                return DeserializationResult.Success;
            }
            return _underlyingDescriptor.DeserializeDirect( data, ctx, out result );
        }

        public object CreateInitialTarget( SerializedData data, SerializationContext ctx )
        {
            if( data == null )
                return (Nullable<T>)null;

            return _underlyingDescriptor.CreateInitialTarget( data, ctx );
        }
    }

    public static class NullableDescriptorProvider
    {
        [MapsInheritingFrom( typeof( Nullable<> ) )]
        public static IDescriptor GetNullableDescriptor<T>( ContextKey context ) where T : struct
        {
            return new NullablePrimitiveDescriptor<T>( context );
        }
    }
}
