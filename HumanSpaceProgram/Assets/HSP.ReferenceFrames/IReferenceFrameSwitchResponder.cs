namespace HSP.ReferenceFrames
{
    public interface IReferenceFrameSwitchResponder
    {
        /// <summary>
        /// Callback to the reference frame switch event.
        /// </summary>
        /// <remarks>
        /// This method will be called AFTER 'PhysicsProcessing', but still in the same fixedupdate as it.
        /// </remarks>
        void OnSceneReferenceFrameSwitch( SceneReferenceFrameManager.ReferenceFrameSwitchData data );
    }
}