
namespace UnityPlus
{
    /// <summary>
    /// Represents an object that can override and block a number of objects of the same type.
    /// </summary>
    public interface IOverridable<T>
    {
        /// <summary>
        /// Gets the ID of this object.
        /// </summary>
        T ID { get; }

        /// <summary>
        /// Gets the list of objects that this objects blocks.
        /// </summary>
        T[] Blacklist { get; }
    }
}