
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

        /// <summary>
        /// Subscribes to the reference frame manager to call the onreferenceframeswitch method.
        /// </summary>
        void SubscribeIfNotSubscribed( IReferenceFrameSwitchResponder responder );
        /// <summary>
        /// Unsubscribes from the reference frame manager to stop calling the onreferenceframeswitch method.
        /// </summary>
        void UnsubscribeIfSubscribed( IReferenceFrameSwitchResponder responder );
    }
}