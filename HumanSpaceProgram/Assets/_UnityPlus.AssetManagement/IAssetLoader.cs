using System;
using System.Threading;
using System.Threading.Tasks;

namespace UnityPlus.AssetManagement
{
    /// <summary>
    /// Responsible for parsing the actual asset contents (retrieved from the handle) into a Unity Object/C# Object.
    /// </summary>
    public interface IAssetLoader
    {
        /// <summary>
        /// Returns the base C# type this loader produces (e.g. typeof(Texture2D), typeof(object), etc).
        /// </summary>
        /// <remarks>
        /// Used for coarse type check, to not call CanLoad on every single loader. <br/>
        /// If the targetType is not assignable to OutputType, this loader is skipped entirely.
        /// </remarks>
        Type OutputType { get; } // evaluate if this is needed. this logic can be handled by the CanLoad method anyway.

        /// <summary>
        /// Determines if this loader can load/read the contents of this handle, reading them as the specific target type.
        /// </summary>
        bool CanLoad( AssetDataHandle handle, Type targetType );

        /// <summary>
        /// Parses the data into the target type.
        /// </summary>
        /// <remarks>
        /// This method is invoked on a separate thread. Implementations are responsible for switching to the main thread when needed. <br/>
        /// This can be done using <see cref="MainThreadDispatcher.RunAsync{T}(Func{T})"/>
        /// </remarks>
        Task<object> LoadAsync( AssetDataHandle handle, Type targetType, CancellationToken ct );
    }
}