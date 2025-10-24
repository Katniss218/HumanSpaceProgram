using UnityEngine.SceneManagement;

namespace HSP.SceneManagement
{
    /// <summary>
    /// An interface that all HSP scenes implement.
    /// </summary>
    /// <remarks>
    /// The members here aren't really important outside the HSP.SceneManagement assembly. <br/>
    /// The caller should be more interested with casting the scene to a specific derived type, or directly accessing the singleton of the scene of interest instead.
    /// </remarks>
    public interface IHSPScene
    {
        /// <summary>
        /// The loaded Unity scene associated with this HSP scene instance
        /// </summary>
        Scene UnityScene { get; }

        void _onload();
        void _onunload();
        void _onactivate();
        void _ondeactivate();
    }

    public interface IHSPScene<TLoadData> : IHSPScene
    {
        /// <summary>
        /// The loaded Unity scene associated with this HSP scene instance
        /// </summary>
        Scene UnityScene { get; }

        void _onload( TLoadData data );
        void _onunload();
        void _onactivate();
        void _ondeactivate();
    }
}