
using System;

namespace UnityPlus.Serialization
{
    public enum SerializationStyle
    {
        None = 0,
        PrimitiveStruct, // save -=- instantiate (called in load)
        PrimitiveObject, // save -=- instantiate (called in loadreferences)
        NonPrimitive     // save -=- instantiate + load, loadreferences
                         //   instantiate and load need to be separate because we need a way to load into an existing object
    }

    public abstract class SerializationMapping
    {
        internal int context;

        public int Context => this.context;

        public abstract SerializationStyle SerializationStyle { get; }

        // The reason these Save/Load/etc. methods use the `object` type instead of being generic,
        //   is that `SerializationMapping<Transform>` can't be cast to `SerializationMapping<Component>`.

        /// <summary>
        /// Saves the full state of the object <paramref name="obj"/>.
        /// </summary>
        public abstract SerializedData Save( object obj, ISaver s );

        /// <summary>
        /// The factory method.
        /// </summary>
        public abstract object Instantiate( SerializedData data, ILoader l );

        /// <summary>
        /// Loads (creates) the object from <paramref name="data"/>.
        /// If the serialization style is Compound, this will recursively create all applicable objects.
        /// </summary>
        public abstract void Load( ref object obj, SerializedData data, ILoader l );

        /// <summary>
        /// Populates the members of the object <paramref name="obj"/>, if applicable.
        /// </summary>
        public abstract void LoadReferences( ref object obj, SerializedData data, ILoader l );

        public virtual SerializationMapping GetWorkingInstance()
        {
            return this;
        }

        public static SerializationMapping Empty( Type sourceType )
        {
            return (SerializationMapping)Activator.CreateInstance( typeof( EmptySerializationMapping<> ).MakeGenericType( sourceType ) );
        }

        public static SerializationMapping Empty<TSource>()
        {
            return new EmptySerializationMapping<TSource>();
        }
    }
}