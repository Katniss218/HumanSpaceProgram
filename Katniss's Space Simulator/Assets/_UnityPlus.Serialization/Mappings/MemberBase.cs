
namespace UnityPlus.Serialization
{
    /// <summary>
    /// Saves and loads part of an object of type <typeparamref name="TSource"/>.
    /// </summary>
    /// <typeparam name="TSource">The type that this item belongs to.</typeparam>
    public abstract class MemberBase<TSource>
    {
        /// <summary>
        /// Saves the member, and returns the <see cref="SerializedData"/> representing it.
        /// </summary>
        public abstract SerializedData Save( TSource source, ISaver s );

        /// <summary>
        /// Instantiates the member from <see cref="SerializedData"/> using the most appropriate mapping for the member type and serialized object's '$type', and assigns it to the member.
        /// </summary>
        public abstract void Load( ref TSource source, SerializedData data, ILoader l );

        public abstract void LoadReferences( ref TSource source, SerializedData data, ILoader l );
    }
}