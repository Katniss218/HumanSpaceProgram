using System;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace UnityPlus.Serialization
{
    /// <summary>
    /// Represents an arbitrary serialization mapping.
    /// </summary>
    public abstract class SerializationMapping
    {
        /*
        
        SerializationMapping, by design, should be able to decide how to serialize/deserialize any value (including null).
        - This means that no shoft-circuiting is possible in the code using the mappings.

        */

        internal int context;
        /// <summary>
        /// Gets the serialization context in which this mapping operates.
        /// </summary>
        public int Context => this.context;

        // Mappings use generic `T` instead of being themselves generic - because `SerializationMapping<Transform>` can't be cast to `SerializationMapping<Component>`.
        // - This CAN NOT be solved using variant interfaces - variance is not supported on `ref` parameters.

        /// <summary>
        /// Saves the full state of the object.
        /// </summary>
        protected abstract bool Save<T>( T obj, ref SerializedData data, ISaver s );

        /// <summary>
        /// 
        /// </summary>
        protected abstract bool TryPopulate<T>( ref T obj, SerializedData data, ILoader l );

        /// <summary>
        /// 
        /// </summary>
        protected abstract bool TryLoad<T>( ref T obj, SerializedData data, ILoader l );

        /// <summary>
        /// 
        /// </summary>
        protected abstract bool TryLoadReferences<T>( ref T obj, SerializedData data, ILoader l );

        /// <summary>
        /// Override this if your mapping contains any additional data (and return copy of the mapping in the overloaded method, but with those additional data fields cleared).
        /// </summary>
        /// <returns>Either itself, or a clone (depending on if the mapping needs to persist data between Load and LoadReferences).</returns>
        public virtual SerializationMapping GetInstance()
        {
            return this;
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        internal bool ___passthroughSave<T>( T obj, ref SerializedData data, ISaver s ) => Save<T>( obj, ref data, s );

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        internal bool ___passthroughPopulate<T>( ref T obj, SerializedData data, ILoader l ) => TryPopulate<T>( ref obj, data, l );

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        internal bool ___passthroughLoad<T>( ref T obj, SerializedData data, ILoader l ) => TryLoad<T>( ref obj, data, l );

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        internal bool ___passthroughLoadReferences<T>( ref T obj, SerializedData data, ILoader l ) => TryLoadReferences<T>( ref obj, data, l );
    }
}