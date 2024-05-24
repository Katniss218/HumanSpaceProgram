
namespace UnityPlus.Serialization
{
    /// <summary>
    /// Represents a member that is a reference to some other member.
    /// </summary>
    /// <typeparam name="TSource">The type that this member belongs to.</typeparam>
    public interface IMappedReferenceMember<TSource>
    {
        SerializedData Save( TSource source, IReverseReferenceMap s );

        void LoadReferences( ref TSource source, SerializedData data, IForwardReferenceMap l );
    }
}