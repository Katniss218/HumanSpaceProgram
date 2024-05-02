using UnityPlus.Serialization;

namespace KSS
{
    /// <summary>
    /// Specifies that the component will be affected by 'ghosting' of the part when constructing.
    /// </summary>
    public interface IGhostable
    {
        /// <returns>The serialized data with the members affected by the 'ghosting', with their corresponding 'ghosted' values.</returns>
        public SerializedData GetGhostData( IReverseReferenceMap s );
    }
}