
namespace UnityEngine
{
    /// <summary>
    /// Implement this interface if you wish to restrict implementing your interface to classes derived from <see cref="Component"/>.
    /// </summary>
    public interface IComponent
    {
        Transform transform { get; }
        GameObject gameObject { get; }
        string tag { get; set; }

        // The rest of the methods and fields defined by Unity can be added too, but I didn't bother.
    }
}