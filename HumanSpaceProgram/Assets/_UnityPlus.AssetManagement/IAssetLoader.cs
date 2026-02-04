using System;
using System.Threading;
using System.Threading.Tasks;

namespace UnityPlus.AssetManagement
{
    /// <summary>
    /// Responsible for parsing the handle into a Unity Object/C# Object.
    /// </summary>
    public interface IAssetLoader
    {
        /// <summary>
        /// Returns the C# type this loader produces (e.g., typeof(Texture2D)).
        /// </summary>
        Type OutputType { get; }

        /// <summary>
        /// Determines if this loader can handle the data.
        /// Checks RequestedType, FormatHint, and optionally peeks magic bytes via handle.
        /// </summary>
        bool CanLoad( AssetDataHandle handle );

        /// <summary>
        /// Parses the data.
        /// Starts on ThreadPool. Implementation is responsible for switching to Main Thread if needed.
        /// </summary>
        Task<object> LoadAsync( AssetDataHandle handle, CancellationToken ct );
    }
}