
namespace UnityPlus
{
    /// <summary>
    /// Represents an object that can be sorted topologically (i.e. before and after other objects, e.g. in a graph).
    /// </summary>
    public interface ITopologicallySortable<T>
    {
        /// <summary>
        /// Gets the ID of THIS object.
        /// </summary>
        T ID { get; }

        /// <summary>
        /// Gets the objects that should end up BEFORE THIS object after sorting.
        /// </summary>
        T[] Before { get; }

        /// <summary>
        /// Gets the objects that should end up AFTER THIS object after sorting.
        /// </summary>
        T[] After { get; }
    }
}