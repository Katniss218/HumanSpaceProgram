
namespace UnityPlus.Serialization
{
    /// <summary>
    /// Saves and loads part of an object of type <typeparamref name="TSource"/>.
    /// </summary>
    /// <typeparam name="TSource">The type that this item belongs to.</typeparam>
    public abstract class MemberBase<TSource>
    {
        public string Name { get; protected set; }

        public abstract MemberBase<TSource> Copy();

        // one is for initial save/load, and the 2nd is for the 'retry' one (if first time failed).
        public abstract SerializationResult Save( TSource sourceObj, SerializedData sourceData, ISaver s, out SerializationMapping mapping, out object memberObj );
        public abstract SerializationResult SaveRetry( object memberObj, SerializationMapping mapping, SerializedData sourceData, ISaver s );

        public abstract SerializationResult Load( ref TSource sourceObj, bool isInstantiated, SerializedData sourceData, ILoader l, out SerializationMapping mapping, out object memberObj );
        public abstract SerializationResult LoadRetry( ref object memberObj, SerializationMapping mapping, SerializedData sourceData, ILoader l );

        /// <summary>
        /// Calls the getter associated with this member.
        /// </summary>
        public abstract object Get( ref TSource sourceObj );

        /// <summary>
        /// Calls the setter associated with this member. <br/>
        /// Does nothing if the member is 'readonly'.
        /// </summary>
        public abstract void Set( ref TSource sourceObj, object member );
    }
}