namespace UnityPlus.Serialization
{
    public interface ICollectionDescriptorWithContext : ICollectionDescriptor
    {
        IContextSelector ElementSelector { get; set; }
    }

    /// <summary>
    /// Describes a resizable collection (Array, List).
    /// </summary>
    public interface ICollectionDescriptor : ICompositeDescriptor
    {
        /// <summary>
        /// Resizes **AND CLEARS** the collection to the new size.
        /// </summary>
        /// <param name="target">The collection to clear and resize.</param>
        /// <param name="newSize">The new capacity of the collection.</param>
        /// <returns>The resized collection (may be a new instance if the collection type doesn't support resizing in-place, e.g. Arrays).</returns>
        object Resize( object target, int newSize );
    }
}