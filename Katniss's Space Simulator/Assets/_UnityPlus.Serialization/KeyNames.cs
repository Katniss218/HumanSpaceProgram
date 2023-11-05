namespace UnityPlus.Serialization
{
    /// <summary>
    /// Contains constants for consistent naming of special keys in serialized data.
    /// </summary>
    public static class KeyNames
    {
        /// <summary>
        /// The special key name for an ID of an object.
        /// </summary>
        /// <remarks>
        /// Used when serializing an object instance that is referenced somewhere else.
        /// </remarks>
        public const string ID = "$id";

        /// <summary>
        /// The special key name for the <see cref="System.Type"/> of an object.
        /// </summary>
        /// <remarks>
        /// Used when serializing an object instance that is part of an inheritance tree.
        /// </remarks>
        public const string TYPE = "$type";

        /// <summary>
        /// The special key name for a reference to an object.
        /// </summary>
        /// <remarks>
        /// Used when serializing a reference to a serialized object.
        /// </remarks>
        public const string REF = "$ref";

        /// <summary>
        /// The special key name for an asset reference.
        /// </summary>
        /// <remarks>
        /// Used when serializing a reference to an asset (see <see cref="UnityPlus.AssetManagement.AssetRegistry"/>).
        /// </remarks>
        public const string ASSETREF = "$assetref";
    }
}