
namespace HSP.ReferenceFrames
{
    /// <summary>
    /// Provides a scene space reference frame.
    /// </summary>
    public interface ISceneReferenceFrameProvider
    {
        /// <summary>
        /// Gets the reference frame that defines the current *scene space* in whichever scene is of interest.
        /// </summary>
        IReferenceFrame GetSceneReferenceFrame();
    }
}