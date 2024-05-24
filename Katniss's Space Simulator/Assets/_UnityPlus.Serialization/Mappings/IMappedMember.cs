
namespace UnityPlus.Serialization
{
    /// <summary>
    /// Represents a member that is owned by the source type.
    /// </summary>
    /// <typeparam name="TSource">The type that this member belongs to.</typeparam>
    public interface IMappedMember<TSource>
    {
        /// <summary>
        /// Saves the member, and returns the <see cref="SerializedData"/> representing it.
        /// </summary>
        SerializedData Save( TSource source, IReverseReferenceMap s );

        /// <summary>
        /// Instantiates the member from <see cref="SerializedData"/> using the most appropriate mapping for the member type and serialized object's '$type', and assigns it to the member.
        /// </summary>
        void Load( ref TSource source, SerializedData data, IForwardReferenceMap l );
    }
}